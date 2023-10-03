using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Database;
using FivePD.Server.Exceptions;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Addons;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Interfaces.LooseData;
using FivePD.Server.Models;
using FivePD.Server.Services;
using FivePD.Server.Services.Addons;
using FivePD.Server.Services.Callouts;
using FivePD.Server.Services.LooseData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Cfx = CitizenFX.Core.Native;
using Debug = CitizenFX.Core.Debug;

namespace FivePD.Server;

/// <summary>
/// The server entrypoint. Handles the
/// creation and management of all tasks on the server.
/// </summary>
// ReSharper disable once UnusedType.Global
internal class Bootstrapper : BaseScript
{
    private readonly Dictionary<Action, bool> _phases = new();
    private readonly Stopwatch _stopwatch = new();
    private readonly ILogger _bootstrapLogger;

    private ServiceProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Bootstrapper" /> class.
    /// Used as the entrypoint for the Cfx server runtime.
    /// </summary>
    public Bootstrapper()
    {
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

        // Enqueue service location phase.
        this._phases.Add(this.PerformServiceLocationPhase, false);

        // Enqueue addon location phase.
        this._phases.Add(this.PerformAddonLocationPhase, false);

        // Enqueue post-bootstrap phase.
        this._phases.Add(this.PerformPostBootstrapPhase, false);

        // Enqueue addon startup phase.
        this._phases.Add(this.PerformAddonEnqueuePhase, false);

        // Enqueue database prefill phase.
        this._phases.Add(this.PrefillDatabase, false);

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
            this._bootstrapLogger.Information("FivePD bootstrapping has completed");

            // Stop trying to run phases
            this.Tick -= this.RunNextPhaseAsync;
            return Task.CompletedTask;
        }

#if DEBUG
        this._bootstrapLogger.Information("Running bootstrap phase: {phase}", phase.Key.Method.Name);
#endif

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
            this._bootstrapLogger.Fatal("An error occurred during bootstrapping, stopping FivePD to prevent further issues.");
            this._bootstrapLogger.Fatal("Exception: {exception}", e.Message);
            this._bootstrapLogger.Debug("Stacktrace: {stacktrace}", e.StackTrace);
            if (e.InnerException is not null)
            {
                this._bootstrapLogger.Debug("Inner exception: {innerException}", e.InnerException);
            }

            return Task.CompletedTask;
        }

        // Once completed, log the phase duration
        this._stopwatch.Stop();
#if DEBUG
        this._bootstrapLogger.Information("Bootstrap phase completed in {duration}ms", this._stopwatch.ElapsedMilliseconds);
#endif

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
        services.AddTransient<IAddonLoggerService, AddonLoggerService>();
        services.AddSingleton<IAddonAssemblyLoader, AddonAssemblyLoader>();
        services.AddSingleton<IAddonFileHandlerService, AddonFileHandlerService>();

        services.AddSingleton<AddonMetadataService>();
        services.AddSingleton<IAddonMetadataService>(provider => provider.GetRequiredService<AddonMetadataService>());
        services.AddSingleton<IInternalAddonMetadataService>(provider => provider.GetRequiredService<AddonMetadataService>());

        // Other
        services.AddSingleton<IFileHandlerService, FileHandlerService>();
        services.AddSingleton<ICommandService, CommandService>();
        services.AddSingleton<IDevToolsService, DevToolsService>();
        services.AddSingleton<IDepartmentService, DepartmentService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IEntityService, EntityService>();
        services.AddSingleton<IMarkerService, MarkerService>();
        services.AddSingleton<IUnitService, UnitService>();
        services.AddSingleton<IConfirmationService, ConfirmationService>();
        services.AddSingleton<IRandomService, RandomService>();
        services.AddSingleton<IAssistanceServiceManagerService, AssistanceServiceManagerService>();
        services.AddSingleton<ICfxService, CfxService>();
        services.AddSingleton<IBackupManagerService, BackupManagerService>();
        services.AddSingleton<IProgressionSystemService, ProgressionSystemService>();
        services.AddSingleton<IMessageService, MessageService>();

        services.AddSingleton<LooseDataService>();
        services.AddSingleton<ILooseDataService>(provider => provider.GetRequiredService<LooseDataService>());
        services.AddSingleton<IInternalLooseDataService>(provider => provider.GetRequiredService<LooseDataService>());

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

        services.AddSingleton<ArrestReportService>();
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<ArrestReportService>());
        services.AddSingleton<IOnDestroy>(provider => provider.GetRequiredService<ArrestReportService>());

        services.AddSingleton<IncidentReportService>();
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<IncidentReportService>());
        services.AddSingleton<IOnDestroy>(provider => provider.GetRequiredService<IncidentReportService>());

        services.AddSingleton<EventService>();
        services.AddSingleton<IEventService>(provider => provider.GetRequiredService<EventService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<EventService>());

        services.AddSingleton<InteractionService>();
        services.AddSingleton<IInteractionService>(provider => provider.GetRequiredService<InteractionService>());
        services.AddSingleton<IInternalInteractionService>(provider => provider.GetRequiredService<InteractionService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<InteractionService>());

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

        services.AddSingleton<MenuService>();
        services.AddSingleton<IMenuService>(provider => provider.GetRequiredService<MenuService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<MenuService>());

        services.AddSingleton<CalloutManagerService>();
        services.AddSingleton<ICalloutManagerService>(provider => provider.GetRequiredService<CalloutManagerService>());
        services.AddSingleton<IInternalCalloutManagerService>(provider => provider.GetRequiredService<CalloutManagerService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<CalloutManagerService>());

        services.AddSingleton<CalloutStoreService>();
        services.AddSingleton<ICalloutStoreService>(provider => provider.GetRequiredService<CalloutStoreService>());
        services.AddSingleton<IInternalCalloutStoreService>(provider => provider.GetRequiredService<CalloutStoreService>());

        services.AddSingleton<CalloutDistributionService>();
        services.AddSingleton<ICalloutDistributionService>(provider => provider.GetRequiredService<CalloutDistributionService>());
        services.AddSingleton<IInternalCalloutDistributionService>(provider => provider.GetRequiredService<CalloutDistributionService>());

        services.AddSingleton<CalloutGenerationService>();
        services.AddSingleton<ICalloutGenerationService>(provider => provider.GetRequiredService<CalloutGenerationService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<CalloutGenerationService>());
        services.AddSingleton<IOnDestroy>(provider => provider.GetRequiredService<CalloutGenerationService>());

        services.AddSingleton<DispatchService>();
        services.AddSingleton<IDispatchService>(provider => provider.GetRequiredService<DispatchService>());
        services.AddSingleton<IInternalDispatchService>(provider => provider.GetRequiredService<DispatchService>());
        services.AddSingleton<IOnInit>(provider => provider.GetRequiredService<DispatchService>());

        services.AddSingleton<IncidentHistoryService>();
        services.AddSingleton<IIncidentHistoryService>(provider => provider.GetRequiredService<IncidentHistoryService>());
        services.AddSingleton<IInternalIncidentHistoryService>(provider => provider.GetRequiredService<IncidentHistoryService>());

        this._provider = services.BuildServiceProvider();

        ServiceLocator.SetServiceProvider(this._provider);
        LifecycleHandler.SetLogger(this._provider.GetService<ILogger>());
    }

    /// <summary>
    /// Locates addons and their definitions.
    /// </summary>
    private void PerformAddonLocationPhase()
    {
        var log = this._provider.GetService<ILogger>()?.ForContext("SourceIdentifier", "AddonLocator");
        if (this._provider is null)
        {
            log?.Fatal("The service provider must be initialized prior to locating addons.");
            LifecycleHandler.Stop();
            return;
        }

        var addonLocator = new FilesystemAddonLocator(Log.Logger.ForContext("SourceIdentifier", "AddonLocator"));
        var addonDefinitions = addonLocator.GetAddons().ToList();
        var addonContainerService = this._provider.GetService<IInternalAddonMetadataService>();
        addonContainerService.LoadAssemblies(addonDefinitions);
        addonContainerService.FindAndRegisterAddons();
    }

    /// <summary>
    /// Performs post-startup tasks.
    /// </summary>
    private void PerformPostBootstrapPhase()
    {
        Debug.WriteLine("PerformPostBootstrapPhase 1");

        var log = this._provider.GetService<ILogger>()?.ForContext("SourceIdentifier", "Bootstrapper");
        Debug.WriteLine("PerformPostBootstrapPhase 2");

        this.LogDiagnostics();

        Debug.WriteLine("PerformPostBootstrapPhase 3");

        if (!this.IsSafeToRun())
        {
            log?.Fatal("FivePD cannot run in this environment. Verify all prerequisites have been met and try again");
            LifecycleHandler.Stop();
            return;
        }

        Debug.WriteLine("PerformPostBootstrapPhase 4");

        // Register shutdown routine listener
        this.EventHandlers["onResourceStop"] += new Action<string>((resource) =>
        {
            if (resource.ToUpper() == "FIVEPD")
            {
                LifecycleHandler.Stop();
            }
        });

        Debug.WriteLine("PerformPostBootstrapPhase 5");

        try
        {
            var db = this._provider.GetService<DatabaseContext>();
            Debug.WriteLine("PerformPostBootstrapPhase 6");
            if (db is null)
            {
                throw new DatabaseUsageException("No database context is available for usage");
            }

            Debug.WriteLine("PerformPostBootstrapPhase 7 --------");

            var migrations = db.Database.GetPendingMigrations().ToList();
            Debug.WriteLine("PerformPostBootstrapPhase 7 -------- after GetPendingMigrations");
            var databaseCreated = db.Database.EnsureCreated();
            Debug.WriteLine("PerformPostBootstrapPhase 7 -------- after EnsureCreated");
            if (!db.Database.CanConnect())
            {
                Debug.WriteLine("PerformPostBootstrapPhase 7 -------- inside CanConnect");
                throw new DatabaseUsageException("Database connection failed");
            }

            Debug.WriteLine("PerformPostBootstrapPhase 8 --------");

            switch (databaseCreated)
            {
                case true:
                {
                    Debug.WriteLine("PerformPostBootstrapPhase 9 TRUE");
                    log?.Information("The database did not exist, it has been created");
                    db.Database.ExecuteSqlRaw(
                        "CREATE TABLE __efmigrationshistory (MigrationId VARCHAR(95) NOT NULL PRIMARY KEY, ProductVersion VARCHAR(32) NOT NULL)");
                    foreach (var migration in migrations)
                    {
                        db.Database.ExecuteSqlRaw("INSERT INTO __efmigrationshistory (MigrationId, ProductVersion) VALUES ({0}, {1})", migration, "3.1.26");
                    }

                    break;
                }

                case false when migrations.Count > 0:
                    Debug.WriteLine("PerformPostBootstrapPhase 9 FALSE");
                    log?.Information("Upgrading the database to {Version}", migrations.Last());
                    db.Database.Migrate();
                    break;
            }

            Debug.WriteLine("PerformPostBootstrapPhase 10");

            log?.Information("The database is up to date");
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException(
                "The configured database is invalid or offline. Verify the connection settings in the /Config/database file and try again.", e);
        }

        Debug.WriteLine("PerformPostBootstrapPhase 11");

        // Register all event listeners for services
        foreach (var service in this._provider.GetServices<IOnInit>())
        {
            service.OnInit();
        }

        Debug.WriteLine("PerformPostBootstrapPhase 12");

        var tickQueueService = this._provider.GetService<IInternalTickQueueService>();
        this.Tick += tickQueueService!.OnTickAsync;
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

    /// <summary>
    /// Enqueues database prefill.
    /// </summary>
    private void PrefillDatabase()
    {
        var log = this._provider.GetService<ILogger>()?.ForContext("SourceIdentifier", "Bootstrapper");

        var databaseContext = this._provider.GetService<DatabaseContext>();
        if (databaseContext is null)
        {
            return;
        }

        if (!databaseContext.Departments.Any())
        {
            var department = new Department
            {
                Name = "Los Santos Police Department",
                Callsign = "LSPD",
                MaxUsers = 0,
                CallsignFormat = "*L*S-NN",
                Ranks = new List<Rank>
                {
                    new()
                    {
                        Name = "Officer I",
                        RequiredXp = 0,
                        Permission = new Rank.Permissions
                        {
                            Kick = false,
                            SetCallsign = false,
                            SetRank = false,
                        },
                    },
                    new()
                    {
                        Name = "Officer II",
                        RequiredXp = 0,
                        Permission = new Rank.Permissions
                        {
                            Kick = false,
                            SetCallsign = false,
                            SetRank = false,
                        },
                    },
                    new()
                    {
                        Name = "Sergeant",
                        RequiredXp = 0,
                        Permission = new Rank.Permissions
                        {
                            Kick = false,
                            SetCallsign = false,
                            SetRank = false,
                        },
                    },
                },
            };
            databaseContext.Departments.Add(department);
            databaseContext.SaveChangesAsync();
            log?.Information("Created sample department");
        }
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

        var log = this._provider.GetService<ILogger>()?.ForContext("SourceIdentifier", "Bootstrapper");
        log?.Information("Stopping FivePD");

        var looseData = this._provider.GetService<ILooseDataService>();
        ((IInternalLooseDataService)looseData).DisposeAddonRepositories();

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
    /// Determines if the resource can run in the current environment.
    /// <returns>A boolean representing the safety of the environment.</returns>
    /// </summary>
    private bool IsSafeToRun()
    {
        // Verifies OneSync is enabled
        if (Cfx.API.GetConvar("onesync", "off") != "on")
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Logs common diagnostic properties.
    /// </summary>
    private void LogDiagnostics()
    {
        var log = this._provider.GetService<ILogger>()!.ForContext("SourceIdentifier", "Bootstrapper");

        log.Debug("Running Cfx version {@Version}", Cfx.API.GetConvar("version", "Unknown"));
    }
}