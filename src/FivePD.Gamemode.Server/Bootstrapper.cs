using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Server.API;
using FivePD.Gamemode.Server.Database;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models;
using FivePD.Gamemode.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server;

/// <summary>
/// The gamemode server entrypoint. Handles the
/// creation and management of all tasks on the server.
/// </summary>
// ReSharper disable once UnusedType.Global
internal class Bootstrapper : BaseScript
{
    private readonly Dictionary<Action, bool> _phases = new();
    private readonly Stopwatch _stopwatch = new();
    private readonly ILogger _bootstrapLogger;

    private ServiceProvider _provider;

    private bool _isStopped;
    private Dictionary<Assembly, AddonDefinition> _assembliesWithDefinitions;

    /// <summary>
    /// Initializes a new instance of the <see cref="Bootstrapper" /> class.
    /// Used as the entrypoint for the Cfx server runtime.
    /// </summary>
    public Bootstrapper()
    {
        // Prepare the server for development if applicable.
#if DEBUG
        ConfigureForDevelopment();
#endif

        // Register logger.
        Log.Logger = new LoggerConfiguration()
            .WriteTo.CfxDebugSink("[{Timestamp:HH:mm:ss} FivePD:{SourceIdentifier} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                $"{AppDomain.CurrentDomain.BaseDirectory}/Logs/FivePD-GS-Log-.txt",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 7)
            .MinimumLevel.Verbose()
            .CreateLogger();

        // Creates log context for bootstrapping.
        this._bootstrapLogger = Log.Logger.ForContext("SourceIdentifier", "Bootstrapper");
        this._bootstrapLogger.Information("Starting FivePD Gamemode Server...");

        // Enqueue pre-bootstrap phase.
        this._phases.Add(this.PerformPreBootstrapPhase, false);

        // Enqueue addon location phase.
        this._phases.Add(this.PerformAddonLocationPhase, false);

        // Enqueue service location phase.
        this._phases.Add(this.PerformServiceLocationPhase, false);

        // Enqueue post-bootstrap phase.
        this._phases.Add(this.PerformPostBootstrapPhase, false);

        // Enqueue addon startup phase.
        this._phases.Add(this.PerformAddonEnqueuePhase, false);

        // Begin running phases.
        this.Tick += this.RunNextPhaseAsync;
    }

    /// <summary>
    /// Terminates the gamemode.
    /// </summary>
    internal void StopGamemode()
    {
        this.OnStopped();
    }

#if DEBUG
    /// <summary>
    /// Configures pre-run settings for development mode.
    /// This logic runs before the gamemode has been initialized.
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
        if (phase.Key == null)
        {
            // Log that we're done
            this._bootstrapLogger.Information("FivePD Gamemode Server bootstrapping has completed");

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
            // Problem? Stop the gamemode.
            this.Tick -= this.RunNextPhaseAsync;

            // Log the problem
            this._bootstrapLogger.Fatal("An error occurred during bootstrapping and the gamemode has halted to prevent further issues.");
            this._bootstrapLogger.Fatal("Exception: {exception}", e.Message);
            this._bootstrapLogger.Debug("Stacktrace: {stacktrace}", e.StackTrace);
            if (e.InnerException != null)
            {
                this._bootstrapLogger.Debug("Inner exception: {innerException}", e.InnerException);
            }

            this._bootstrapLogger.Fatal("There is an error above that caused FivePD to stop");

            return Task.CompletedTask;
        }

        // Once completed, log the phase duration
        this._stopwatch.Stop();
        this._bootstrapLogger.Information("Bootstrap phase completed in {duration}ms", this._stopwatch.ElapsedMilliseconds);

        // Mark the phase as completed
        this._phases[phase.Key] = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Performs pre-startup tasks.
    /// </summary>
    private void PerformPreBootstrapPhase()
    {
        // todo: validate certs for sentry and our in-house api
#pragma warning disable S4830
        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
#pragma warning restore S4830
    }

    /// <summary>
    /// Locates addons and their definitions.
    /// </summary>
    private void PerformAddonLocationPhase()
    {
        /*
         * Gamemode addons have full(ish) access to the complete suite of services
         * made available to core gamemode services. Thus, we need to find our addons
         * prior to actually assembling a service this._provider. The addon locator takes one
         * parameter, a logger, so we create it early.
         */

        this._bootstrapLogger.Information(
            "Bootstrapping gamemode server version {@Version}",
            Assembly.GetExecutingAssembly().GetName().Version.ToString());

        var addonLocator = new FilesystemAddonLocator(Log.Logger.ForContext("SourceIdentifier", "AddonLocator"));

        this._assembliesWithDefinitions = new Dictionary<Assembly, AddonDefinition>();

        var allAddons = addonLocator.GetAddons().ToList();

        // Load addon assemblies
        foreach (var addon in allAddons)
        {
            this._bootstrapLogger.Information(
                "Registering addon \"{@AddonName}\" version {@AddonVersion}",
                addon.FriendlyName,
                addon.FriendlyVersion);

            foreach (var assembly in addon.Assemblies)
            {
                var asm = Assembly.Load(assembly);
                this._assembliesWithDefinitions.Add(asm, addon);
            }
        }
    }

    /// <summary>
    /// Locates and registers services.
    /// </summary>
    private void PerformServiceLocationPhase()
    {
        var services = new ServiceCollection();

        // Find and register core services
        services.AddSingleton(Log.Logger);
        services.AddSingleton(this.EventHandlers);
        services.AddSingleton(this.Players);
        services.AddDbContext<DatabaseContext>();

        services.AddSingleton<IAddonMetadataService, AddonMetadataService>();
        services.AddTransient<IAddonLoggerService, AddonLoggerService>();
        services.AddSingleton<IAddonAssemblyLoader, AddonAssemblyLoader>();
        services.AddSingleton<IAddonFileHandlerService, AddonFileHandlerService>();

        services.AddSingleton<IHttpService, HttpService>();
        services.AddSingleton<IFileHandlerService, FileHandlerService>();
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<ICommandService, CommandService>();
        services.AddSingleton<IDevToolsService, DevToolsService>();
        services.AddSingleton<ILooseDataService, LooseDataService>();
        services.AddSingleton<IDepartmentService, DepartmentService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IEntityService, EntityService>();
        services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
        services.AddSingleton<IMarkerService, MarkerService>();

#pragma warning disable
        // !! SERVICES WITH EVENTS SHOULD BE SINGLETONS
        // !! AND SHOULD BE REGISTERED AS THE ONES BELOW THIS COMMENT !!
#pragma warning enable

        services.AddSingleton<ConcurrentTickQueueService>();
        services.AddSingleton<ITickQueueService>(provider => provider.GetRequiredService<ConcurrentTickQueueService>());
        services.AddSingleton<IInteralTickQueueService>(provider => provider.GetRequiredService<ConcurrentTickQueueService>());
        
        services.AddSingleton<UIService>();
        services.AddSingleton<IUIService>(provider => provider.GetRequiredService<UIService>());
        services.AddSingleton<IServiceWithEvents>(provider => provider.GetRequiredService<UIService>());

        services.AddSingleton<ControlService>();
        services.AddSingleton<IControlService>(provider => provider.GetRequiredService<ControlService>());
        services.AddSingleton<IServiceWithEvents>(provider => provider.GetRequiredService<ControlService>());

        services.AddSingleton<LocalizationService>();
        services.AddSingleton<ILocalizationService>(provider => provider.GetRequiredService<LocalizationService>());
        services.AddSingleton<IServiceWithEvents>(provider => provider.GetRequiredService<LocalizationService>());

        services.AddSingleton<ReportService>();
        services.AddSingleton<IReportService>(provider => provider.GetRequiredService<ReportService>());
        services.AddSingleton<IServiceWithEvents>(provider => provider.GetRequiredService<ReportService>());

        services.AddSingleton<EventService>();
        services.AddSingleton<IEventService>(provider => provider.GetRequiredService<EventService>());
        services.AddSingleton<IServiceWithEvents>(provider => provider.GetRequiredService<EventService>());

        services.AddSingleton<InteractionService>();
        services.AddSingleton<IInteractionService>(provider => provider.GetRequiredService<InteractionService>());
        services.AddSingleton<IServiceWithEvents>(provider => provider.GetRequiredService<InteractionService>());

        services.AddSingleton<PedService>();
        services.AddSingleton<IPedService>(provider => provider.GetRequiredService<PedService>());
        services.AddSingleton<IServiceWithEvents>(provider => provider.GetRequiredService<PedService>());

        services.AddSingleton<VehicleService>();
        services.AddSingleton<IVehicleService>(provider => provider.GetRequiredService<VehicleService>());
        services.AddSingleton<IServiceWithEvents>(provider => provider.GetRequiredService<VehicleService>());

        services.AddSingleton<PlayerDataService>();
        services.AddSingleton<IPlayerDataService>(provider => provider.GetRequiredService<PlayerDataService>());
        services.AddSingleton<IServiceWithEvents>(provider => provider.GetRequiredService<PlayerDataService>());

        services.AddSingleton<MenuService>();
        services.AddSingleton<IMenuService>(provider => provider.GetRequiredService<MenuService>());
        services.AddSingleton<IServiceWithEvents>(provider => provider.GetRequiredService<MenuService>());

        // Find and register addons
        services.Scan(scan => scan
            .FromAssemblies(this._assembliesWithDefinitions.Keys)
            .AddClasses(classes => classes.AssignableTo<IGamemodeAddon>())
            .AsSelfWithInterfaces()
            .WithSingletonLifetime());

        this._provider = services.BuildServiceProvider();

        ServiceLocator.SetServiceProvider(this._provider);
    }

    /// <summary>
    /// Performs post-startup tasks.
    /// </summary>
    private void PerformPostBootstrapPhase()
    {
        var addonMetadataService = this._provider.GetService<IAddonMetadataService>();
        foreach (var assembly in this._assembliesWithDefinitions)
        {
            addonMetadataService?.RegisterAddonAssemblyWithDefinition(assembly.Key, assembly.Value);
        }

        var log = this._provider.GetService<ILogger>()?.ForContext("SourceIdentifier", "Bootstrapper");

        this.LogDiagnostics();

        if (!this.IsSafeToRun())
        {
            log?.Fatal("FivePD cannot run in this environment. Verify all prerequisites have been met and try again");
            this.OnStopped();
            return;
        }

        // Register shutdown routine listener
        this.EventHandlers["onResourceStop"] += new Action(this.OnStopped);

        try
        {
            var db = this._provider.GetService<DatabaseContext>();
            if (db is null)
            {
                throw new DatabaseUsageException("No database context is available for usage");
            }

            if (!db.Database.CanConnect())
            {
                throw new DatabaseUsageException("Database connection failed");
            }

            var migrations = db.Database.GetPendingMigrations();

            try
            {
                log?.Information("Upgrading the database to {Version}", migrations.Last());
                db.Database.Migrate();
            }
            catch (Exception)
            {
                // Throws when there are no migrations to run. Not an issue. Continue.
                log?.Information("The database is up to date");
            }
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException(
                "The configured database is invalid or offline. Verify the connection settings in the /Config/database.json file and try again.", e);
        }

        // Register all event listeners for services
        foreach (var service in this._provider.GetServices<IServiceWithEvents>())
        {
            log?.Debug("Registering event handlers for {Service}", service.GetType());

            service.RegisterEvents();
        }

        var tickQueueService = this._provider.GetService<IInteralTickQueueService>();
        this.Tick += tickQueueService!.OnTickAsync;
    }

    /// <summary>
    /// Enqueues addon startup tasks.
    /// </summary>
    private void PerformAddonEnqueuePhase()
    {
        var tickQueueService = this._provider.GetService<ITickQueueService>();

        // Grab an instance of every addon entrypoint
        foreach (var addon in this._provider.GetServices<IGamemodeAddon>())
        {
            // Lazy load it
            tickQueueService.EnqueueAsync(() => addon.OnStartedAsync());
        }
    }

    /// <summary>
    /// Called when the gamemode is being stopped. Handles cleanup.
    /// </summary>
    private void OnStopped()
    {
        if (this._isStopped)
        {
            return;
        }

        var log = this._provider.GetService<ILogger>()?.ForContext("SourceIdentifier", "Bootstrapper");

        log?.Information("Stopping FivePD");

        this._isStopped = true;

        log?.Debug("Running addon shutdown routine");

        // Grab an instance of every addon entrypoint and "stop" it
        foreach (var addon in this._provider.GetServices<IGamemodeAddon>())
        {
            addon.OnStoppedAsync();
        }

        log?.Debug("Addon shutdown complete. Running gamemode disposal routine");

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
    /// Determines if the gamemode can run in the current environment.
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

        // Logs the current Cfx artifacts version
        log.Debug(
            "Running Cfx version {@Version}",
            Cfx.API.GetConvar("version", "Unknown"));

        // Logs dev mode status
        var devModeEnabled = Cfx.API.GetConvar("sv_enableFivepdDevUtils", "false") == "true";

        log.Debug(
            "Development mode is {@Status}",
            devModeEnabled ? "on" : "off");

        if (devModeEnabled)
        {
            log.Warning(
                "Do not enable development mode on public servers. Only enable development mode when developing addons for FivePD. Development mode allows bad actors to exploit FivePD. This is VERY dangerous");
        }
    }

    // TEMPORARY: Use while we do not have a proper way to go on duty.
    // /setduty <0,1,2> corresponding to an FDutyStatus value
    [Command("setduty")]
    private void TemporarySetDutyCommand(Player cfxPlayer, string[] args)
    {
        var player = FPlayer.FromNetId(cfxPlayer.Handle);
        player.DutyStatus.Value = (FDutyStatus)uint.Parse(args[0]);
    }
}