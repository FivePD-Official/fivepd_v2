using System;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.DTO;
using FivePD.Common;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Interfaces.TrafficStopInterfaces;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc cref="FivePD.Server.Interfaces.IInteractionService" />
public class InteractionService : IInteractionService, IInternalInteractionService, IOnInit
{
    private readonly IPedService _pedService;
    private readonly IVehicleService _vehicleService;
    private readonly IControlService _controlService;
    private readonly IEventService _eventService;
    private readonly ITickQueueService _tickQueueService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ICfxService _cfxService;
    private readonly ITrafficStopService _trafficStopService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionService"/> class.
    /// </summary>
    /// <param name="pedService">The <see cref="IPedService"/> to use.</param>
    /// <param name="vehicleService">The <see cref="IVehicleService"/> to use.</param>
    /// <param name="controlService">The <see cref="IControlService"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="cfxService">The <see cref="ICfxService"/> to use.</param>
    /// <param name="trafficStopService">The <see cref="ITrafficStopService"/> to use.</param>
    public InteractionService(
        IPedService pedService,
        IVehicleService vehicleService,
        IControlService controlService,
        IEventService eventService,
        ITickQueueService tickQueueService,
        EventHandlerDictionary eventHandlerDictionary,
        ICfxService cfxService,
        ITrafficStopService trafficStopService)
    {
        this._pedService = pedService;
        this._vehicleService = vehicleService;
        this._controlService = controlService;
        this._eventService = eventService;
        this._tickQueueService = tickQueueService;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._cfxService = cfxService;
        this._trafficStopService = trafficStopService;
    }

    /// <inheritdoc />
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.Ped.OnAlerted] += new Action<Player, int>(this.OnPedAlerted);
        this._eventHandlerDictionary[Events.Vehicle.OnAlerted] += new Action<Player, string>(this.OnVehicleAlerted);
    }

    private async void OnPedAlerted([FromSource] Player player, int pedNetworkId)
    {
        var ped = this._pedService.TryGetPed(pedNetworkId);
        if (ped is null)
        {
            return;
        }

        if (ped.Properties.IsInVehicle || ped.Stopped.Value)
        {
            return;
        }

        ped.Stopped.Value = true;

        Cfx.API.SetEntityDistanceCullingRadius(ped.GetUnderlyingPed.Handle, 10000f);

        ped.Properties.SetAlwaysKeepTask(true);
        ped.Properties.SetBlockPermanentEvents(true);

        ped.Task.StandStill(-1);
        ped.Task.TurnTo(player.Character);

        Debug.WriteLine($"Player with net ID of {player.Character.NetworkId} alerted ped with net ID of {ped.NetworkId}");
    }

    private async void OnVehicleAlerted([FromSource] Player player, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<OnVehicleAlertedDTO>(serializedRequest);
        if (request is null)
        {
            return;
        }

        var fVehicle = this._vehicleService.TryGetVehicle(request.VehicleNetworkId);
        if (fVehicle is null || fVehicle.Stopped.Value)
        {
            return;
        }

        Cfx.API.SetEntityDistanceCullingRadius(fVehicle.GetUnderlyingVehicle.Handle, 10000f);
        fVehicle.Stopped.Value = true;

        foreach (var pedNetworkId in request.OccupantNetworkIds)
        {
            var ped = this._pedService.TryGetPed(pedNetworkId);
            if (ped?.GetUnderlyingPed is null)
            {
                continue;
            }

            Cfx.API.SetEntityDistanceCullingRadius(ped.GetUnderlyingPed.Handle, 10000f);

            ped.Properties.SetAlwaysKeepTask(true);
            ped.Properties.SetBlockPermanentEvents(true);
        }

        var driverPed = this._pedService.TryGetPed(request.DriverPedNetworkId);
        if (driverPed is null || !driverPed.Properties.IsInVehicle)
        {
            return;
        }

        driverPed.Task.ClearAll();
        this._cfxService.TaskVehicleTempAction(driverPed.NetworkId, fVehicle.NetworkId, 24, int.MaxValue);

        if (request.CreateTrafficStopInstance)
        {
            var fPlayer = FPlayer.FromPlayer(player);
            this._trafficStopService.GenerateTrafficStop(fPlayer.Unit, fVehicle);
        }

        Debug.WriteLine($"Player with net ID of {player.Character.NetworkId} alerted vehicle with net ID of {request.VehicleNetworkId}");
    }

    void IInternalInteractionService.TogglePedFollowPlayer(FPlayer player, Ped ped, bool follow)
    {
        this._eventService.TriggerEventWithoutCallback(ped.Owner, Events.Interaction.ToggleFollowPlayer, ped.NetworkId, follow);
    }
}