// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Injection;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Services;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S3241
#pragma warning disable CS4014
namespace FivePD.Gamemode.Client
{
    /// <summary>
    /// The gamemode client entrypoint. Handles the
    /// creation and management of all tasks on the client.
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class Bootstrapper : BaseScript
    {
        private readonly Dictionary<Action, bool> _phases = new Dictionary<Action, bool>();
        private readonly ILoggerService _bootstrapLogger;

        private ServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bootstrapper" />
        /// class. Used as the entrypoint for the Cfx runtime.
        /// </summary>
        public Bootstrapper()
        {
            // Initialize the logger.
            this._bootstrapLogger = new LoggerService();

            this._bootstrapLogger.Info("Starting FivePD Gamemode Client...");

            // Enqueue service location phase.
            this._phases.Add(this.PerformServiceLocationPhase, false);

            // Enqueue service initialization phase.
            this._phases.Add(this.PerformInitializationPhase, false);

            // Begin running phases.
            this.Tick += this.RunNextPhase;
        }

        /// <summary>
        /// Runs the next phase in the bootstrap process.
        /// Will unregister the tick event if all phases
        /// have been completed.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes
        /// when a phase is ran.</returns>
        private Task RunNextPhase()
        {
            // Get the first phase that isn't completed (second parameter is not true)
            var phase = this._phases.FirstOrDefault(x => !x.Value);

            // If no phase is found, we're done
            if (phase.Key == null)
            {
                // Log that we're done
                this._bootstrapLogger.Info("FivePD Gamemode Client bootstrapping has completed");

                // Stop trying to run phases
                this.Tick -= this.RunNextPhase;
                return Task.FromResult(0);
            }

            #if DEBUG
            this._bootstrapLogger.Info("Running bootstrap phase: {phase}", phase.Key.Method.Name);
            #endif

            // Run the phase
            try
            {
                phase.Key();
            }
            catch (Exception e)
            {
                // Problem? Stop the gamemode.
                this.Tick -= this.RunNextPhase;

                // Log the problem
                this._bootstrapLogger.Error("An error occurred during bootstrapping and the gamemode has halted to prevent further issues.");
                this._bootstrapLogger.Info("Exception: {exception}", e.Message);
                this._bootstrapLogger.Error("Stacktrace: {stacktrace}", e.StackTrace);
                if (e.InnerException != null)
                {
                    this._bootstrapLogger.Info("Inner exception: {innerException}", e.InnerException);
                }

                this._bootstrapLogger.Error("There is an error above that caused FivePD to stop");

                return Task.FromResult(0);
            }

            // Mark the phase as completed
            this._phases[phase.Key] = true;

            return Task.FromResult(0);
        }

        private void PerformServiceLocationPhase()
        {
            var services = new ServiceProviderBuilder();

            services
                .Register(() => this.EventHandlers)
                .As<EventHandlerDictionary>();

            services
                .Register<AddTickHandler>(() => this.OnTickAdd);
            services
                .Register<RemoveTickHandler>(() => this.OnTickRemove);

            services
                .RegisterType<FileHandlerService>()
                .As<IFileHandlerService>();

            services
                .RegisterType<LoggerService>()
                .As<ILoggerService>();

            services
                .RegisterType<LocalizationService>()
                .As<ILocalizationService>()
                .IsSingleton();

            services
                .RegisterType<DevUtilitiesService>()
                .As<IDevUtilitiesService>()
                .IsSingleton();

            services
                .RegisterType<ConfigService>()
                .As<IConfigService>();

            services
                .RegisterType<EntityService>()
                .As<IEntityService>();

            services
                .RegisterType<TickService>()
                .As<ITickService>();

            services
                .RegisterType<ScreenDecorService>()
                .As<IScreenDecorService>();

            services
                .RegisterType<NuiService>()
                .As<INuiService>()
                .IsSingleton();

            services
                .RegisterType<SoundService>()
                .As<ISoundService>();

            services
                .RegisterType<ControlService>()
                .As<IControlService>()
                .IsSingleton();

            services
                .RegisterType<RadialMenuService>()
                .As<IRadialMenuService>();

            services
                .RegisterType<MenuService>()
                .As<IMenuService>();

            services
                .RegisterType<PedService>()
                .As<IPedService>();

            services
                .RegisterType<VehicleService>()
                .As<IVehicleService>();

            services
                .RegisterType<NotificationService>()
                .As<INotificationService>();

            services
                .RegisterType<PoliceEventService>()
                .As<IPoliceEventService>()
                .IsSingleton();

            services
                .RegisterType<TrafficStopService>()
                .As<ITrafficStopService>();

            services
                .RegisterType<LocalPlayerService>()
                .As<ILocalPlayerService>();

            services
                .RegisterType<InteractionService>()
                .As<IInteractionService>()
                .IsSingleton();

            this._serviceProvider = services.Build();
        }

        private void PerformInitializationPhase()
        {
            // Load localizations EARLY!!!
            var localizationService = this._serviceProvider.Resolve<ILocalizationService>();
            localizationService.RegisterEventHandlers();
            localizationService.LoadDefaultLocalization();

            var screenDecorService = this._serviceProvider.Resolve<IScreenDecorService>();
            screenDecorService.Version = "v2.0.0";
            screenDecorService.ProtectionNotice = true;
            screenDecorService.BeginTextDraw();

            var keybindService = this._serviceProvider.Resolve<IControlService>();
            keybindService.Register(this._serviceProvider);

            var nuiService = this._serviceProvider.Resolve<INuiService>();
            nuiService.RegisterEventHandlers();

            var radialMenuService = this._serviceProvider.Resolve<IRadialMenuService>();
            radialMenuService.RegisterEvents();
            radialMenuService.CreateMenus();
            radialMenuService.RegisterMenuKeybinds();

            var menuService = this._serviceProvider.Resolve<IMenuService>();
            menuService.RegisterEvents();
            menuService.CreateMenus();
            menuService.RegisterMenuKeybinds();

            var interactionService = this._serviceProvider.Resolve<IInteractionService>();
            interactionService.RegisterKeybinds();
        }

        private void OnTickAdd(Func<Task> task)
        {
            this.Tick += task;
        }

        private void OnTickRemove(Func<Task> task)
        {
            this.Tick -= task;
        }
    }
}