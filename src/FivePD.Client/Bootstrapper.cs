using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Injection;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Client.Services;
using FivePD.Common;
using FivePD.Common.Utilities;
using CfxUI = CitizenFX.Core.UI;

#pragma warning disable S3241
#pragma warning disable CS4014

#pragma warning disable S125
#pragma warning disable S1481
#pragma warning disable SA1512
#pragma warning disable S1450

namespace FivePD.Client;

/// <summary>
/// The client entrypoint. Handles the
/// creation and management of all tasks on the client.
/// </summary>
// ReSharper disable once UnusedType.Global
internal class Bootstrapper : BaseScript
{
    private readonly Dictionary<Action, bool> _phases = new();
    private readonly ILoggerService _bootstrapLogger;

    private ServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Bootstrapper" />
    /// class. Used as the entrypoint for the Cfx runtime.
    /// </summary>
    public Bootstrapper()
    {
        this._bootstrapLogger = new LoggerService();

        TriggerServerEvent(Events.Player.OnConnected);

        this.EventHandlers[Events.Lifecycle.Stop] += () =>
            LifecycleHandler.Stop("An error occured or the FivePD resource has been stopped on the server. Stopping FivePD.");

        this.EventHandlers[Events.Lifecycle.ServerBootstrapped] += LifecycleHandler.ServerBootstrapped;

        this.EventHandlers[Events.AccessControl.Allow] += this.AccessAllowed;
        this.EventHandlers[Events.AccessControl.Disallow] +=
            () => LifecycleHandler.Stop("Your access has been removed from using FivePD. Stopping FivePD.");

        this.RunBootstrap();

        TriggerServerEvent(Events.AccessControl.IsAllowed, new Action<bool>(allowed =>
        {
            if (allowed)
            {
                return;
            }

            LifecycleHandler.Stop("You do not have access to use FivePD. Stopping FivePD.");
        }));
    }

    private void AccessAllowed()
    {
        CfxUI.Screen.ShowNotification(new GtaStyleText()
            .Blue("[FivePD]")
            .NewLine()
            .Green("You have been given access to use FivePD.")
            .Build());
        this.RunBootstrap();
    }

    private void RunBootstrap()
    {
        this._bootstrapLogger.Info("Starting FivePD...");

        this._phases.Clear();

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
            this._bootstrapLogger.Info("FivePD bootstrapping has completed");

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var notificationService = this._serviceProvider.Resolve<INotificationService>();
            notificationService.ShowNotification(new GtaStyleText()
                .Blue("[FivePD]")
                .NewLine()
                .White("Bootstrapping has been completed.")
                .NewLine()
                .Orange($"Version: {version.Major}.{version.Minor}.{version.Build}"));

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
            this.Tick -= this.RunNextPhaseAsync;

            // Log the problem
            this._bootstrapLogger.Error("An error occurred during bootstrapping and FivePD has halted to prevent further issues.");
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
            .Register(() => this.Players)
            .As<PlayerList>();

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
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<FileHandlerService>()
            .As<IFileHandlerService>();

        services
            .RegisterType<LoggerService>()
            .As<ILoggerService>();

        services
            .RegisterType<DevToolsService>()
            .As<IDevToolsService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<ConfigService>()
            .As<IConfigService>();

        services
            .RegisterType<TickService>()
            .As<ITickService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<ScreenDecorService>()
            .As<IScreenDecorService>()
            .As<IOnDestroy>()
            .IsSingleton();

        services
            .RegisterType<UIManagerService>()
            .As<IUIManagerService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<UIService>()
            .As<IUIService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<AudioService>()
            .As<IAudioService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<ControlService>()
            .As<IControlService>()
            .As<IOnInit>()
            .As<IOnDestroy>()
            .IsSingleton();

        services
            .RegisterType<MenuService>()
            .As<IMenuService>()
            .As<IOnInit>()
            .IsSingleton();

        // TODO: I'm not sure about radial menus.
        // services
        //     .RegisterType<RadialMenuService>()
        //     .As<IRadialMenuService>();

        services
            .RegisterType<PedService>()
            .As<IPedService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<VehicleService>()
            .As<IVehicleService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<NotificationService>()
            .As<INotificationService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<LocalPlayerService>()
            .As<ILocalPlayerService>()
            .As<IOnInit>()
            .As<IOnDestroy>()
            .IsSingleton();

        services
            .RegisterType<InteractionService>()
            .As<IInteractionService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<MarkerService>()
            .As<IMarkerService>()
            .As<IOnInit>()
            .As<IOnDestroy>()
            .IsSingleton();

        services
            .RegisterType<ConfirmationService>()
            .As<IConfirmationService>()
            .As<IOnInit>()
            .As<IOnDestroy>()
            .IsSingleton();

        services
            .RegisterType<CalloutManagerService>()
            .As<ICalloutManagerService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<IncidentService>()
            .As<IIncidentService>()
            .As<IOnInit>()
            .As<IOnDestroy>()
            .IsSingleton();

        services
            .RegisterType<CfxService>()
            .As<ICfxService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<DispatchService>()
            .As<IDispatchService>()
            .As<IOnInit>()
            .As<IOnDestroy>()
            .IsSingleton();

        services
            .RegisterType<DepartmentService>()
            .As<IDepartmentService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<VariableDataService>()
            .As<IVariableDataService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<UnitService>()
            .As<IUnitService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<IncidentHistoryService>()
            .As<IIncidentHistoryService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<EntityService>()
            .As<IEntityService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<MessageService>()
            .As<IMessageService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<QueueService>()
            .As<IQueueService>()
            .As<IOnInit>()
            .As<IOnDestroy>()
            .IsSingleton();

        services
            .RegisterType<VehicleConfigService>()
            .As<IVehicleConfigService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<LoadoutConfigService>()
            .As<ILoadoutConfigService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<WeaponService>()
            .As<IWeaponService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<WeaponComponentService>()
            .As<IWeaponComponentService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<LocationService>()
            .As<ILocationService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<Vector3Service>()
            .As<IVector3Service>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<AlprService>()
            .As<IAlprService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<SceneManagementService>()
            .As<ISceneManagementService>()
            .As<IOnInit>()
            .IsSingleton();

        services
            .RegisterType<AccessControlService>()
            .As<IAccessControlService>()
            .As<IOnInit>()
            .IsSingleton();

        this._serviceProvider = services.Build();

        ServiceLocator.SetServiceProvider(this._serviceProvider);
    }

    private void PerformInitializationPhase()
    {
        var serviceWithEvents = this._serviceProvider.ResolveAll<IOnInit>();
        foreach (var service in serviceWithEvents)
        {
#if DEBUG
            this._bootstrapLogger.Info($"Registering event handlers for {service.GetType()}");
#endif

            try
            {
                service.OnInit();
            }
            catch (Exception e)
            {
                this._bootstrapLogger.Error(e.Message);
            }
        }
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