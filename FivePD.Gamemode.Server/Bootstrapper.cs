// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
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

            var services = new ServiceCollection();

            services.AddSerilogServices(
                new LoggerConfiguration()
                    .WriteTo.Console(
                        theme: AnsiConsoleTheme.Code,
                        outputTemplate: "[{Timestamp:HH:mm:ss} FivePD {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(
                        $"{AppDomain.CurrentDomain.BaseDirectory}/Logs/Server-.txt",
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 7)
                    .MinimumLevel.Verbose());

            services.AddSingleton<ITickQueueService, ConcurrentTickQueueService>();
            services.AddSingleton<IAddonLocator, FsAddonLocator>();
            services.AddSingleton<IEntityService, EntityService>();
            services.AddSingleton<IServerStateService, ServerStateService>();
            services.AddSingleton<IReplicatedStateService, ReplicatedStateService>();
            services.AddSingleton<IDevUtilitiesService, DevUtilitiesService>();
            services.AddSingleton<ILocalizationService, LocalizationService>();

            services.AddSingleton<IPlayerDataService, PlayerDataService>();

            services.AddSingleton(this.EventHandlers);

            var provider = services.BuildServiceProvider();

            var localizationService = provider.GetService<ILocalizationService>();
            localizationService?.RegisterEventHandlers();
            localizationService?.LoadAvailableLocales();
            localizationService?.ValidateLocalizationFiles();

            var log = provider.GetService<ILogger>();

            log?.Information(
                "Bootstrapping gamemode server version {@Version}",
                Assembly.GetExecutingAssembly().GetName().Version.ToString());

            LogDiagnostics(log);

            // Check for dev mode. If enabled, register the utilities
            if (Cfx.API.GetConvar("sv_enableFivepdDevUtils", "false") == "true")
            {
                var devUtilsService = provider.GetService<IDevUtilitiesService>();

                devUtilsService?.RegisterDevelopmentCommands();
            }

            var tickQueueService = provider.GetService<ITickQueueService>();
            this.Tick += tickQueueService!.OnTick;

            var addonLocator = provider.GetService<IAddonLocator>();

            var allAddons = addonLocator?.GetAddons().ToList();

            log?.Information("Found {@Count} addons, passing to addon loader", allAddons?.Count());

            // Load internal addons. MUST COMPLETE prior to loading public addons.
            foreach (var addon in allAddons?.Where(x => x.IsInternal)!)
            {
                log?.Information(
                    "Loading addon {@AddonName}",
                    addon.FriendlyName);

                foreach (var assembly in addon.Assemblies)
                {
                    var asm = Assembly.Load(assembly);

                    foreach (var type in asm.GetTypes())
                    {
                        log?.Information("Found type {@Type}", type.FullName);
                    }
                }
            }

            // Load public addons.

            // Demonstrate player data service
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