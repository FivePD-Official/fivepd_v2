using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Injection;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Services;
using FivePD.Gamemode.Common;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S3241
#pragma warning disable CS4014

#pragma warning disable S125
#pragma warning disable S1481
#pragma warning disable SA1512
#pragma warning disable S1450

namespace FivePD.Gamemode.Client;

/// <summary>
/// The gamemode client entrypoint. Handles the
/// creation and management of all tasks on the client.
/// </summary>
// ReSharper disable once UnusedType.Global
internal class Bootstrapper : BaseScript
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
        TriggerServerEvent(Events.Player.OnConnected);

        // Initialize the logger.
        this._bootstrapLogger = new LoggerService();

        this._bootstrapLogger.Info("Starting FivePD Gamemode Client...");

        // Enqueue service location phase.
        this._phases.Add(this.PerformServiceLocationPhase, false);

        // Enqueue service initialization phase.
        this._phases.Add(this.PerformInitializationPhase, false);

        // Begin running phases.
        this.Tick += this.RunNextPhaseAsync;
    }

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
            this._bootstrapLogger.Info("FivePD Gamemode Client bootstrapping has completed");

            // Stop trying to run phases
            this.Tick -= this.RunNextPhaseAsync;
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
            this.Tick -= this.RunNextPhaseAsync;

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
        // !! SERVICES WITH EVENTS SHOULD BE SINGLETONS !!

        var services = new ServiceProviderBuilder();

        services
            .Register(() => this.EventHandlers)
            .As<EventHandlerDictionary>();

        services
            .Register<AddTickHandler>(() => this.OnTickAdd);
        services
            .Register<RemoveTickHandler>(() => this.OnTickRemove);

        services
            .RegisterType<LocalizationService>()
            .As<ILocalizationService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        services
            .RegisterType<FileHandlerService>()
            .As<IFileHandlerService>();

        services
            .RegisterType<LoggerService>()
            .As<ILoggerService>();

        services
            .RegisterType<DevUtilitiesService>()
            .As<IDevUtilitiesService>()
            .IsSingleton();

        services
            .RegisterType<ConfigService>()
            .As<IConfigService>();

        services
            .RegisterType<TickService>()
            .As<ITickService>();

        services
            .RegisterType<ScreenDecorService>()
            .As<IScreenDecorService>()
            .IsSingleton();

        services
            .RegisterType<UIService>()
            .As<IUIService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        services
            .RegisterType<SoundService>()
            .As<ISoundService>();

        services
            .RegisterType<ControlService>()
            .As<IControlService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        services
            .RegisterType<MenuService>()
            .As<IMenuService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        // TODO: I'm not sure about radial menus.
        // services
        //     .RegisterType<RadialMenuService>()
        //     .As<IRadialMenuService>();

        services
            .RegisterType<PedService>()
            .As<IPedService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        services
            .RegisterType<VehicleService>()
            .As<IVehicleService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        services
            .RegisterType<NotificationService>()
            .As<INotificationService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        services
            .RegisterType<LocalPlayerService>()
            .As<ILocalPlayerService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        services
            .RegisterType<InteractionService>()
            .As<IInteractionService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        services
            .RegisterType<MarkerService>()
            .As<IMarkerService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        services
            .RegisterType<ConfirmationService>()
            .As<IConfirmationService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        services
            .RegisterType<CalloutManagerService>()
            .As<ICalloutManagerService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        services
            .RegisterType<CfxService>()
            .As<ICfxService>()
            .As<IServiceWithEvents>()
            .IsSingleton();

        this._serviceProvider = services.Build();

        ServiceLocator.SetServiceProvider(this._serviceProvider);
    }

    private void PerformInitializationPhase()
    {
        var screenDecorService = this._serviceProvider.Resolve<IScreenDecorService>();
        screenDecorService.Version = "v2.0.0";
        screenDecorService.ProtectionNotice = true;

        var serviceWithEvents = this._serviceProvider.ResolveAll<IServiceWithEvents>();
        foreach (var service in serviceWithEvents)
        {
#if DEBUG
            this._bootstrapLogger.Info($"Registering event handlers for {service.GetType()}");
#endif

            try
            {
                service.RegisterEvents();
            }
            catch (Exception e)
            {
                this._bootstrapLogger.Error(e.Message);
            }
        }

        screenDecorService.BeginTextDraw();

        var localizationService = this._serviceProvider.Resolve<ILocalizationService>();
        localizationService.LocalizationChanged += (_, _) => { screenDecorService.BuildText(); };
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