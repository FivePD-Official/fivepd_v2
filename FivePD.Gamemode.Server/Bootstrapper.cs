// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.API;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server
{
    /**
     * <summary>The gamemode server entrypoint. Handles the creation and management of all tasks on the server.</summary>
     */
    // ReSharper disable once UnusedType.Global
    public class Bootstrapper : BaseScript
    {
        /**
         * <summary>
         *     Initializes a new instance of the <see cref="Bootstrapper" /> class. Used as the entrypoint for the Cfx
         *     runtime.
         * </summary>
         */
        public Bootstrapper()
        {
            #if DEBUG
            ConfigureForDevelopment();
            #endif

            var globalLoggerConfig = new LoggerConfiguration()
                .WriteTo.Console(
                    theme: AnsiConsoleTheme.Code,
                    outputTemplate: "[{Timestamp:HH:mm:ss} FivePD:{SourceIdentifier} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    $"{AppDomain.CurrentDomain.BaseDirectory}/Logs/Server-.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 7)
                .MinimumLevel.Verbose();

            /*
             * Gamemode addons have full(ish) access to the complete suite of services
             * made available to core gamemode services. Thus, we need to find our addons
             * prior to actually assembling a service provider. The addon locator takes one
             * parameter, a logger, so we create it early.
             */

            Log.Logger = globalLoggerConfig.CreateLogger();
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();

            var preInitLogger = Log.Logger.ForContext("SourceIdentifier", "Bootstrapper");

            preInitLogger.Information(
                "Bootstrapping gamemode server version {@Version}",
                Assembly.GetExecutingAssembly().GetName().Version.ToString());

            var addonLocator = new FilesystemAddonLocator(Log.Logger.ForContext("SourceIdentifier", "AddonLocator"));

            var assemblies = new List<Assembly>();

            var allAddons = addonLocator.GetAddons().ToList();

            // Load addon assemblies
            foreach (var addon in allAddons)
            {
                preInitLogger.Information(
                    "Registering addon {@AddonName} version {@AddonVersion}",
                    addon.FriendlyName,
                    addon.FriendlyVersion);

                assemblies.AddRange(addon.Assemblies.Select(Assembly.Load));
            }

            var services = new ServiceCollection();

            // Find and register core services
            services.AddSingleton(Log.Logger);

            services.AddTransient<IAddonLoggerService, AddonLoggerService>();
            services.AddSingleton<IResourceService, ResourceService>();
            services.AddSingleton<ITickQueueService, ConcurrentTickQueueService>();
            services.AddSingleton<IAddonAssemblyLoader, AddonAssemblyLoader>();
            services.AddSingleton<IEntityService, EntityService>();
            services.AddSingleton<IServerStateService, ServerStateService>();
            services.AddSingleton<IReplicatedStateService, ReplicatedStateService>();
            services.AddSingleton<IDevUtilitiesService, DevUtilitiesService>();
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<IPedService, PedService>();
            services.AddSingleton<IVehicleService, VehicleService>();
            services.AddSingleton<ITrafficStopService, TrafficStopService>();

            services.AddSingleton<IPlayerDataService, PlayerDataService>();

            services.AddSingleton(this.EventHandlers);

            // Find and register addons
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo<IGamemodeAddon>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime());

            var provider = services.BuildServiceProvider();

            var entityService = provider.GetService<IEntityService>();
            entityService?.RegisterEvents();

            var pedService = provider.GetService<IPedService>();
            pedService?.RegisterEvents();

            var vehicleService = provider.GetService<IVehicleService>();
            vehicleService?.RegisterEvents();

            var trafficStopService = provider.GetService<ITrafficStopService>();
            trafficStopService?.RegisterEvents();

            var localizationService = provider.GetService<ILocalizationService>();
            localizationService?.RegisterEventHandlers();
            localizationService?.LoadAvailableLocales();
            localizationService?.ValidateLocalizationFiles();

            var log = provider.GetService<ILogger>()?.ForContext("SourceIdentifier", "Bootstrapper");

            LogDiagnostics(log);

            // Check for dev mode. If enabled, register the utilities
            if (Cfx.API.GetConvar("sv_enableFivepdDevUtils", "false") == "true")
            {
                var devUtilsService = provider.GetService<IDevUtilitiesService>();

                devUtilsService?.RegisterDevelopmentCommands();
            }

            var tickQueueService = provider.GetService<ITickQueueService>();
            this.Tick += tickQueueService!.OnTick;

            // Grab an instance of every addon entrypoint
            foreach (var addon in provider.GetServices<IGamemodeAddon>())
            {
                // Lazy load it
                tickQueueService.Enqueue(() => addon.OnStarted());
            }

            // Being that the thread is about to be free for the first time, enqueued addon starts will run here
            log?.Information("Gamemode server bootstrapping has completed. Preparing addon initialization");

            var playerDataService = provider.GetService<IPlayerDataService>()!;
            var demoPlayer = new FPlayer("1");

            playerDataService.DutyStatusChanged += (sender, args) => log.Information("A duty status changed! ({@Player})", args);

            log?.Information("Before duty status is {@Status}", playerDataService.GetDutyStatus(demoPlayer));
            playerDataService.SetDutyStatus(demoPlayer, FDutyStatus.OnDutyAndReceiving);
            log?.Information("After duty status is {@Status}", playerDataService.GetDutyStatus(demoPlayer));
        }

        /**
         * Logs common diagnostic properties
         * <param name="log">The logger to use.</param>
         */
        private static void LogDiagnostics(ILogger log)
        {
            // Logs the current Cfx artifacts version
            log.Debug(
                "Running Cfx version {@Version}",
                Cfx.API.GetConvar("version", "Unknown"));

            // Logs the current OneSync status
            log.Debug(
                "OneSync is {@Status}",
                Cfx.API.GetConvar("onesync", "Unknown"));

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

        #if DEBUG
        /**
         * Sets up some Cfx settings for development only
         */
        private static void ConfigureForDevelopment()
        {
            // Disables client-side script entity spawning
            Cfx.API.SetRoutingBucketEntityLockdownMode(0, "relaxed");
        }
        #endif
    }
}