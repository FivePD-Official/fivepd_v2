using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Addons;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Interfaces.Dispatcher;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Interfaces.TrafficStopInterfaces;
using FivePD.Server.Interfaces.VariableData;
using FivePD.Server.Services;
using FivePD.Server.Services.Addons;
using FivePD.Server.Services.Callouts;
using FivePD.Server.Services.Dispatcher;
using FivePD.Server.Services.TrafficStopServices;
using FivePD.Server.Services.VariableData;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using Cfx = CitizenFX.Core.Native;
using Debug = CitizenFX.Core.Debug;

namespace FivePD.Server;

/// <summary>
/// The server entrypoint. Handles the
/// creation and management of all tasks on the server.
/// </summary>
// ReSharper disable once UnusedType.Global
[Obfuscate]
internal class Bootstrapper : BaseScript
{
    private readonly Dictionary<Action, bool> _phases = new();
    private readonly Stopwatch _stopwatch = new();
    private readonly ILogger _bootstrapLogger;
    private readonly ILogger _addonLocatorLogger;

    private ServiceProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Bootstrapper" /> class.
    /// Used as the entrypoint for the Cfx server runtime.
    /// Used as the entrypoint for the Cfx server runtime.
    /// </summary>
    public Bootstrapper()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            this._bootstrapLogger?.Error("AppDomain unhandled exception: {exception}", args.ExceptionObject);
        };

        LifecycleHandler.OnStopped += (_, _) => this.OnStopped();

        // Prepare the server for development if applicable.
#if DEBUG
        ConfigureForDevelopment();
#endif

        // Register logger.
        Log.Logger = new LoggerConfiguration()
            .WriteTo.CfxDebugSink("[{Timestamp:HH:mm:ss} FivePD:{SourceIdentifier} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                $"{AppDomain.CurrentDomain.BaseDirectory}/Logs/FivePD-Log-.txt",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 7)
            .MinimumLevel.Verbose()
            .CreateLogger();

        // Creates log context for bootstrapping.
        this._bootstrapLogger = Log.Logger.ForContext("SourceIdentifier", "Bootstrapper");
        this._bootstrapLogger.Information("Starting FivePD...");

        this._addonLocatorLogger = Log.Logger.ForContext("SourceIdentifier", "AddonLocator");

        // Enqueue service location phase.
        this._phases.Add(this.PerformServiceLocationPhase, false);

        // Enqueue addon location phase.
        this._phases.Add(this.PerformAddonLocationPhase, false);

        // // Enqueue post-bootstrap phase.
        this._phases.Add(this.PerformPostBootstrapPhase, false);

        this._phases.Add(() => DatabaseHandler.SetParameters(this._provider, this._bootstrapLogger), false);

        this._phases.Add(DatabaseHandler.EnsureCorrectState, false);

        this._phases.Add(this.PerformServiceInitialization, false);

        // Enqueue addon startup phase.
        this._phases.Add(this.PerformAddonEnqueuePhase, false);

        this._phases.Add(DatabaseHandler.SeedData, false);

        this._phases.Add(this.PerformSafeToRunCheckAfterBootstrapping, false);

        // Register menus after addons are loaded.
        this._phases.Add(
            () =>
            {
                var menuService = this._provider.GetService<IInternalMenuService>();
                menuService.RegisterMenus();
            },
            false);

        // Notify all players that bootstrapping is complete.
        this._phases.Add(
            () =>
            {
                var playerList = this._provider.GetService<PlayerList>();
                foreach (var player in playerList)
                {
                    player.TriggerEvent(Events.Lifecycle.ServerBootstrapped);
                }
            },
            false);

        // Register the feedback handler.
        this._phases.Add(
            () =>
            {
                var eventHandlerDictionary = this._provider.GetService<EventHandlerDictionary>();
                eventHandlerDictionary[Events.Feedback.Send] += FeedbackHandler.SendFeedback;
            },
            false);

        // Begin running phases.
        this.Tick += this.RunNextPhaseAsync;
    }

#if DEBUG
    /// <summary>
    /// Configures pre-run settings for development mode.
    /// This logic runs before everything else has been initialized.
    /// </summary>
    private static void ConfigureForDevelopment()
    {
        // Disables client-side script entity spawning
        Cfx.API.SetRoutingBucketEntityLockdownMode(0, "relaxed");
    }
#endif

    /// <summary>
    /// Runs the next phase in the bootstrap process.
    /// Will unregister the tick event if all phases
    /// have been completed.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes
    /// when a phase is ran.</returns>
    private Task RunNextPhaseAsync()
    {
        // Get the first phase that isn't completed (second parameter is not true)
        var phase = this._phases.FirstOrDefault(x => !x.Value);

        // If no phase is found, we're done
        if (phase.Key is null)
        {
            // Log that we're done
            if (!LifecycleHandler.IsStopped)
            {
                this._bootstrapLogger.Information("FivePD bootstrapping has completed");
            }

            // Stop trying to run phases
            this.Tick -= this.RunNextPhaseAsync;
            return Task.CompletedTask;
        }

        // Begin timing the phase
        this._stopwatch.Restart();

        // Run the phase
        try
        {
            phase.Key();
        }
        catch (Exception e)
        {
            this.Tick -= this.RunNextPhaseAsync;

            // Log the problem
            this._bootstrapLogger?.Fatal("An error occurred during bootstrapping, stopping FivePD to prevent further issues.");
            this._bootstrapLogger?.Fatal("Exception: {exception}", e.Message);
            this._bootstrapLogger?.Debug("Stacktrace: {stacktrace}", e.StackTrace);
            if (e.InnerException is not null)
            {
                this._bootstrapLogger?.Debug("Inner exception: {innerException}", e.InnerException);
            }

            return Task.CompletedTask;
        }

        // Once completed, log the phase duration
        this._stopwatch.Stop();

        // Mark the phase as completed
        this._phases[phase.Key] = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Locates and registers services.
    /// </summary>
    private void PerformServiceLocationPhase()
    {
        this._bootstrapLogger.Debug("Bootstrapping server version {@Version}", Assembly.GetExecutingAssembly().GetName().Version.ToString());

        var services = new ServiceCollection();

        // Find and register core services
        services.AddSingleton(Log.Logger);
        services.AddSingleton(this.EventHandlers);
        services.AddSingleton(this.Players);
        services.AddDbContext<DatabaseContext>();

        // Addons
        services.AddTransient<AddonLoggerService>();
        services.AddTransient<IAddonLoggerService>(provider => provider.GetRequiredService<AddonLoggerService>());

        services.AddSingleton<AddonAssemblyLoader>();
        services.AddTransient<IAddonAssemblyLoader>(provider => provider.GetRequiredService<AddonAssemblyLoader>());

        services.AddSingleton<AddonFileHandlerService>();
        services.AddTransient<IAddonFileHandlerService>(provider => provider.GetRequiredService<AddonFileHandlerService>());

        services.AddSingleton<AddonMetadataService>();
        services.AddSingleton<IAddonMetadataService>(provider => provider.GetRequiredService<AddonMetadataService>());
        services.AddSingleton<IInternalAddonMetadataService>(provider => provider.GetRequiredService<AddonMetadataService>());

        // Loose data repositories
        services.AddSingleton<ArrestReportService>();
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<ArrestReportService>());
        services.AddSingleton<IOnDestroy>(provider => provider.GetRequiredService<ArrestReportService>());

        services.AddSingleton<IncidentReportService>();
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<IncidentReportService>());
        services.AddSingleton<IOnDestroy>(provider => provider.GetRequiredService<IncidentReportService>());

        services.AddSingleton<PedTicketService>();
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<PedTicketService>());
        services.AddSingleton<IOnDestroy>(provider => provider.GetRequiredService<PedTicketService>());

        services.AddSingleton<PedWarningService>();
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<PedWarningService>());
        services.AddSingleton<IOnDestroy>(provider => provider.GetRequiredService<PedWarningService>());

        // Other
        services.AddSingleton<FileHandlerService>();
        services.AddSingleton<IFileHandlerService>(provider => provider.GetRequiredService<FileHandlerService>());

        services.AddSingleton<CommandService>();
        services.AddSingleton<ICommandService>(provider => provider.GetRequiredService<CommandService>());

        services.AddSingleton<DevToolsService>();
        services.AddSingleton<IDevToolsService>(provider => provider.GetRequiredService<DevToolsService>());

        services.AddSingleton<DepartmentService>();
        services.AddSingleton<IDepartmentService>(provider => provider.GetRequiredService<DepartmentService>());

        services.AddSingleton<NotificationService>();
        services.AddSingleton<INotificationService>(provider => provider.GetRequiredService<NotificationService>());

        services.AddSingleton<EntityService>();
        services.AddSingleton<IEntityService>(provider => provider.GetRequiredService<EntityService>());

        services.AddSingleton<MarkerService>();
        services.AddSingleton<IMarkerService>(provider => provider.GetRequiredService<MarkerService>());

        services.AddSingleton<ConfirmationService>();
        services.AddSingleton<IConfirmationService>(provider => provider.GetRequiredService<ConfirmationService>());

        services.AddSingleton<RandomService>();
        services.AddSingleton<IRandomService>(provider => provider.GetRequiredService<RandomService>());

        services.AddSingleton<AssistanceServiceManagerService>();
        services.AddSingleton<IAssistanceServiceManagerService>(provider => provider.GetRequiredService<AssistanceServiceManagerService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<AssistanceServiceManagerService>());

        services.AddSingleton<CfxService>();
        services.AddSingleton<ICfxService>(provider => provider.GetRequiredService<CfxService>());

        services.AddSingleton<ProgressionSystemService>();
        services.AddSingleton<IProgressionSystemService>(provider => provider.GetRequiredService<ProgressionSystemService>());

        services.AddSingleton<MessageService>();
        services.AddSingleton<IMessageService>(provider => provider.GetRequiredService<MessageService>());

        services.AddSingleton<UnitManagerService>();
        services.AddSingleton<IUnitManagerService>(provider => provider.GetRequiredService<UnitManagerService>());
        services.AddSingleton<IInternalUnitManagerService>(provider => provider.GetRequiredService<UnitManagerService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<UnitManagerService>());

        services.AddSingleton<UnitService>();
        services.AddSingleton<IUnitService>(provider => provider.GetRequiredService<UnitService>());

        services.AddSingleton<BackupManagerService>();
        services.AddSingleton<IBackupManagerService>(provider => provider.GetRequiredService<BackupManagerService>());

        services.AddSingleton<VariableDataService>();
        services.AddSingleton<IVariableDataService>(provider => provider.GetRequiredService<VariableDataService>());
        services.AddSingleton<IInternalVariableDataService>(provider => provider.GetRequiredService<VariableDataService>());

        services.AddSingleton<ConfigService>();
        services.AddSingleton<IConfigService>(provider => provider.GetRequiredService<ConfigService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<ConfigService>());

        services.AddSingleton<ConcurrentTickQueueService>();
        services.AddSingleton<ITickQueueService>(provider => provider.GetRequiredService<ConcurrentTickQueueService>());
        services.AddSingleton<IInternalTickQueueService>(provider => provider.GetRequiredService<ConcurrentTickQueueService>());

        services.AddSingleton<UIService>();
        services.AddSingleton<IUIService>(provider => provider.GetRequiredService<UIService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<UIService>());

        services.AddSingleton<ControlService>();
        services.AddSingleton<IControlService>(provider => provider.GetRequiredService<ControlService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<ControlService>());

        services.AddSingleton<LocalizationService>();
        services.AddSingleton<ILocalizationService>(provider => provider.GetRequiredService<LocalizationService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<LocalizationService>());

        services.AddSingleton<EventService>();
        services.AddSingleton<IEventService>(provider => provider.GetRequiredService<EventService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<EventService>());

        services.AddSingleton<InteractionService>();
        services.AddSingleton<IInteractionService>(provider => provider.GetRequiredService<InteractionService>());
        services.AddSingleton<IInternalInteractionService>(provider => provider.GetRequiredService<InteractionService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<InteractionService>());

        services.AddSingleton<PedStorageService>();
        services.AddSingleton<IPedStorageService>(provider => provider.GetRequiredService<PedStorageService>());
        services.AddSingleton<IInternalPedStorageService>(provider => provider.GetRequiredService<PedStorageService>());

        services.AddSingleton<VehicleStorageService>();
        services.AddSingleton<IVehicleStorageService>(provider => provider.GetRequiredService<VehicleStorageService>());
        services.AddSingleton<IInternalVehicleStorageService>(provider => provider.GetRequiredService<VehicleStorageService>());

        services.AddSingleton<PedService>();
        services.AddSingleton<IPedService>(provider => provider.GetRequiredService<PedService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<PedService>());
        services.AddSingleton<IOnDestroy>(provider => provider.GetRequiredService<PedService>());

        services.AddSingleton<VehicleService>();
        services.AddSingleton<IVehicleService>(provider => provider.GetRequiredService<VehicleService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<VehicleService>());

        services.AddSingleton<PlayerDataService>();
        services.AddSingleton<IPlayerDataService>(provider => provider.GetRequiredService<PlayerDataService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<PlayerDataService>());

        services.AddSingleton<PlayerHandlerService>();
        services.AddSingleton<IPlayerHandlerService>(provider => provider.GetRequiredService<PlayerHandlerService>());

        services.AddSingleton<MenuService>();
        services.AddSingleton<IMenuService>(provider => provider.GetRequiredService<MenuService>());
        services.AddSingleton<IInternalMenuService>(provider => provider.GetRequiredService<MenuService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<MenuService>());

        services.AddSingleton<CalloutManagerService>();
        services.AddSingleton<ICalloutManagerService>(provider => provider.GetRequiredService<CalloutManagerService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<CalloutManagerService>());

        services.AddSingleton<IncidentService>();
        services.AddSingleton<IIncidentService>(provider => provider.GetRequiredService<IncidentService>());
        services.AddSingleton<IInternalIncidentService>(provider => provider.GetRequiredService<IncidentService>());

        services.AddSingleton<CalloutDistributionService>();
        services.AddSingleton<ICalloutDistributionService>(provider => provider.GetRequiredService<CalloutDistributionService>());
        services.AddSingleton<IInternalCalloutDistributionService>(provider => provider.GetRequiredService<CalloutDistributionService>());

        services.AddSingleton<CalloutGenerationService>();
        services.AddSingleton<ICalloutGenerationService>(provider => provider.GetRequiredService<CalloutGenerationService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<CalloutGenerationService>());
        services.AddSingleton<IOnDestroy>(provider => provider.GetRequiredService<CalloutGenerationService>());

        services.AddSingleton<DispatcherUiService>();
        services.AddSingleton<IDispatcherUiService>(provider => provider.GetRequiredService<DispatcherUiService>());

        services.AddSingleton<DispatcherStorageService>();
        services.AddSingleton<IDispatcherStorageService>(provider => provider.GetRequiredService<DispatcherStorageService>());

        services.AddSingleton<DispatcherManagerService>();
        services.AddSingleton<IDispatcherManagerService>(provider => provider.GetRequiredService<DispatcherManagerService>());
        services.AddSingleton<IInternalDispatcherManagerService>(provider => provider.GetRequiredService<DispatcherManagerService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<DispatcherManagerService>());

        services.AddSingleton<IncidentHistoryService>();
        services.AddSingleton<IIncidentHistoryService>(provider => provider.GetRequiredService<IncidentHistoryService>());
        services.AddSingleton<IInternalIncidentHistoryService>(provider => provider.GetRequiredService<IncidentHistoryService>());

        services.AddSingleton<TrafficStopService>();
        services.AddSingleton<ITrafficStopService>(provider => provider.GetRequiredService<TrafficStopService>());

        services.AddSingleton<CalloutDetailService>();
        services.AddSingleton<ICalloutDetailService>(provider => provider.GetRequiredService<CalloutDetailService>());

        services.AddSingleton<VehicleConfigService>();
        services.AddSingleton<IVehicleConfigService>(provider => provider.GetRequiredService<VehicleConfigService>());

        services.AddSingleton<LoadoutConfigService>();
        services.AddSingleton<ILoadoutConfigService>(provider => provider.GetRequiredService<LoadoutConfigService>());

        services.AddSingleton<WeaponService>();
        services.AddSingleton<IWeaponService>(provider => provider.GetRequiredService<WeaponService>());

        services.AddSingleton<WeaponComponentService>();
        services.AddSingleton<IWeaponComponentService>(provider => provider.GetRequiredService<WeaponComponentService>());

        services.AddSingleton<LocationService>();
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<LocationService>());
        services.AddSingleton<ILocationService>(provider => provider.GetRequiredService<LocationService>());

        services.AddSingleton<Vector3Service>();
        services.AddSingleton<IVector3Service>(provider => provider.GetRequiredService<Vector3Service>());

        services.AddSingleton<AlprService>();
        services.AddSingleton<IAlprService>(provider => provider.GetRequiredService<AlprService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<AlprService>());

        services.AddSingleton<SceneManagementService>();
        services.AddSingleton<ISceneManagementService>(provider => provider.GetRequiredService<SceneManagementService>());
        services.AddSingleton<IInternalSceneManagementService>(provider => provider.GetRequiredService<SceneManagementService>());

        services.AddSingleton<AudioService>();
        services.AddSingleton<IAudioService>(provider => provider.GetRequiredService<AudioService>());

        services.AddSingleton<AccessControlService>();
        services.AddSingleton<IAccessControlService>(provider => provider.GetRequiredService<AccessControlService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<AccessControlService>());

        services.AddSingleton<RankService>();
        services.AddSingleton<IRankService>(provider => provider.GetRequiredService<RankService>());

        services.AddSingleton<GameNotificationService>();
        services.AddSingleton<IGameNotificationService>(provider => provider.GetRequiredService<GameNotificationService>());

        services.AddSingleton<OllamaService>();
        services.AddSingleton<IOllamaService>(provider => provider.GetRequiredService<OllamaService>());

        this._provider = services.BuildServiceProvider();

        ServiceLocator.SetServiceProvider(this._provider);
        LifecycleHandler.SetLogger(this._provider.GetService<ILogger>());
    }

    /// <summary>
    /// Locates addons and their definitions.
    /// </summary>
    private void PerformAddonLocationPhase()
    {
        if (this._provider is null)
        {
            this._addonLocatorLogger?.Fatal("The service provider must be initialized prior to locating addons.");
            LifecycleHandler.Stop();
            return;
        }

        var addonLocator = new FilesystemAddonLocator(this._addonLocatorLogger);
        var addonDefinitions = addonLocator.GetAddons().ToList();
        var addonMetadataService = this._provider.GetService<IInternalAddonMetadataService>();
        addonMetadataService.LoadAssemblies(addonDefinitions);
        addonMetadataService.FindAndRegisterAddons();
    }

    /// <summary>
    /// Performs post-startup tasks.
    /// </summary>
    private void PerformPostBootstrapPhase()
    {
        this.LogDiagnostics();

        // Register shutdown routine listener
        this.EventHandlers["onResourceStop"] += new Action<string>((resource) =>
        {
            if (resource.ToUpper() == "FIVEPD")
            {
                LifecycleHandler.Stop();
            }
        });

        var tickQueueService = this._provider.GetService<IInternalTickQueueService>();
        this.Tick += tickQueueService!.OnTickAsync;
    }

    /// <summary>
    /// Invokes the OnInit method of services registered with the IOnInit interface.
    /// </summary>
    private void PerformServiceInitialization()
    {
        foreach (var service in this._provider.GetServices<IOnInit>())
        {
            service.OnInit();
        }
    }

    /// <summary>
    /// Enqueues addon startup tasks.
    /// </summary>
    private void PerformAddonEnqueuePhase()
    {
        var tickQueueService = this._provider.GetService<ITickQueueService>();

        // Grab an instance of every addon entrypoint
        foreach (var extension in this._provider.GetService<IInternalAddonMetadataService>().Extensions)
        {
            // Lazy load it
            tickQueueService.EnqueueAsync(() => extension.OnStartedAsync());
        }
    }

    private void PerformSafeToRunCheckAfterBootstrapping()
    {
        var errors = new List<string>();

        var (eulaAccepted, eulaError) = LicenseHandler.ValidateEula();
        if (!eulaAccepted)
        {
            errors.Add(eulaError);
        }

        if (Cfx.API.GetConvar("onesync", "off") != "on")
        {
            errors.Add("OneSync needs to be enabled.");
        }

        var localizationService = this._provider.GetService<ILocalizationService>();
        if (localizationService.AvailableLocalizations.Count == 0)
        {
            errors.Add("Ensure you have at least one localization file.");
        }

        var configService = this._provider.GetService<IConfigService>();
        if (string.IsNullOrEmpty(configService.GeneralConfig.DefaultLanguage))
        {
            errors.Add("Ensure the \"DefaultLanguage\" option in the general config is not empty.");
        }
        else
        {
            if (localizationService.AvailableLocalizations.All(x => x.Key != configService.GeneralConfig.DefaultLanguage))
            {
                var availableLocalizations = string.Join(", ", localizationService.AvailableLocalizations.Select(x => x.Key));
                errors.Add(
                    $"Ensure the \"DefaultLanguage\" option in the general config is a valid localization. Available localizations: {availableLocalizations}");
            }
        }

        if (errors.Count == 0)
        {
            return;
        }

        this._bootstrapLogger?.Fatal("FivePD cannot run in this environment. Verify the following:");
        foreach (var message in errors)
        {
            this._bootstrapLogger?.Fatal($"{message}");
        }

        LifecycleHandler.Stop();
    }

    /// <summary>
    /// Called when the resource is being stopped. Handles cleanup.
    /// </summary>
    private void OnStopped()
    {
        if (LifecycleHandler.IsStopped)
        {
            return;
        }

        this._bootstrapLogger?.Information("Stopping FivePD");

        var variableDataService = this._provider.GetService<IVariableDataService>();
        ((IInternalVariableDataService)variableDataService).DisposeAddonRepositories();

        // Dispose all internal services with the interface IDisposableService
        foreach (var service in this._provider.GetServices<IOnDestroy>())
        {
            service.OnDestroy();
        }

        // Grab an instance of every addon entrypoint and "stop" it
        foreach (var extension in this._provider.GetService<IInternalAddonMetadataService>().Extensions)
        {
            extension.OnStoppedAsync();
        }

        // Notify every player that the resource is stopping and prevent any actions on client-side
        foreach (var player in this.Players)
        {
            player.TriggerEvent(Events.Lifecycle.Stop);
        }

        try
        {
            this._provider.Dispose();
        }
        catch (Exception)
        {
            // Doesn't matter at this point
        }
    }

    /// <summary>
    /// Logs common diagnostic properties.
    /// </summary>
    private void LogDiagnostics()
    {
        this._bootstrapLogger?.Debug("Running Cfx version {@Version}", Cfx.API.GetConvar("version", "Unknown"));
    }
}