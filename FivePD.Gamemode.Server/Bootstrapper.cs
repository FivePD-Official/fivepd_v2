using System;
using System.Reflection;
using System.Resources;
using CitizenFX.Core;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using API = CitizenFX.Core.Native.API;

namespace FivePD.Gamemode.Server
{
    /**
     * The gamemode server entrypoint. Handles the creation and management of all tasks on the server.
     */
    public class Bootstrapper : BaseScript
    {
        public Bootstrapper()
        {
            var services = new ServiceCollection();

            services.AddSerilogServices(
                new LoggerConfiguration()
                    .WriteTo.Console(
                        theme: AnsiConsoleTheme.Code,
                        outputTemplate: "[{Timestamp:HH:mm:ss} FivePD {Level:u3}] {Message:lj}{NewLine}{Exception}"
                    ) 
                    .WriteTo.File(
                        $"{AppDomain.CurrentDomain.BaseDirectory}/Logs/Server-.txt",
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 7
                    )
                    .MinimumLevel.Verbose()
            );

            services.AddSingleton<IEntityService, EntityService>();
            services.AddSingleton(EventHandlers);
            
            var provider = services.BuildServiceProvider();
            
            var log = provider.GetService<ILogger>();
            
            log.Information(
                "Bootstrapping gamemode server version {@Version}",
                Assembly.GetExecutingAssembly().GetName().Version.ToString()
            );

            var spawnService = provider.GetService<IEntityService>();
            
            spawnService.RegisterEventHandlers();;
            
            LogDiagnostics(log);
        }

        /**
         * Logs common diagnostic properties
         * <param name="log">The logger to use</param>
         */
        private static void LogDiagnostics(ILogger log)
        {
            // Logs the current Cfx artifacts version
            log.Debug(
                "Running Cfx version {@Version}",
                API.GetConvar("version", "Unknown")
            );
            
            // Logs the current OneSync status
            log.Debug(
                "OneSync status is {@Status}",
                API.GetConvar("onesync", "Unknown")
            );
        }
    }
}