// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Reflection;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Debug = System.Diagnostics.Debug;

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

            services.AddSingleton<IEntityService, EntityService>();
            services.AddSingleton(this.EventHandlers);

            var provider = services.BuildServiceProvider();

            var log = provider.GetService<ILogger>();

            log?.Information(
                "Bootstrapping gamemode server version {@Version}",
                Assembly.GetExecutingAssembly().GetName().Version.ToString());

            var spawnService = provider.GetService<IEntityService>();

            spawnService?.RegisterEventHandlers();

            LogDiagnostics(log);
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
                API.GetConvar("version", "Unknown"));

            // Logs the current OneSync status
            log.Debug(
                "OneSync status is {@Status}",
                API.GetConvar("onesync", "Unknown"));
        }
    }
}