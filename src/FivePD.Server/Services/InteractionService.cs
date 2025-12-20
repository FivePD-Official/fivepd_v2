using System;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Interfaces.TrafficStopInterfaces;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc cref="FivePD.Server.Interfaces.IInteractionService" />
[Obfuscate]
public class InteractionService : IInteractionService, IInternalInteractionService, IOnInit
{
    private readonly IPedStorageService _pedStorageService;
    private readonly IVehicleStorageService _vehicleStorageService;
    private readonly IEventService _eventService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ICfxService _cfxService;
    private readonly ITrafficStopService _trafficStopService;
    private readonly IPedBehaviorService _pedBehaviorService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionService"/> class.
    /// </summary>
    /// <param name="pedStorageService">The <see cref="IPedService"/> to use.</param>
    /// <param name="vehicleStorageService">The <see cref="IVehicleStorageService"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="cfxService">The <see cref="ICfxService"/> to use.</param>
    /// <param name="trafficStopService">The <see cref="ITrafficStopService"/> to use.</param>
    /// <param name="pedBehaviorService"></param>
    public InteractionService(
        IPedStorageService pedStorageService,
        IVehicleStorageService vehicleStorageService,
        IEventService eventService,
        EventHandlerDictionary eventHandlerDictionary,
        ICfxService cfxService,
        ITrafficStopService trafficStopService,
        IPedBehaviorService pedBehaviorService)
    {
        this._pedStorageService = pedStorageService;
        this._vehicleStorageService = vehicleStorageService;
        this._eventService = eventService;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._cfxService = cfxService;
        this._trafficStopService = trafficStopService;
        this._pedBehaviorService = pedBehaviorService;
    }

    /// <inheritdoc />
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.Ped.OnAlerted] += new Action<Player, int>(this.OnPedAlerted);
        this._eventHandlerDictionary[Events.Vehicle.OnAlerted] += new Action<Player, string>(this.OnVehicleAlerted);
    }

    private void OnPedAlerted([FromSource] Player player, int pedNetworkId)
    {
        var ped = this._pedStorageService.TryGetPed(pedNetworkId);
        if (ped is null)
        {
            return;
        }

        if (ped.Stopped.Value)
        {
            return;
        }

        ped.Stopped.Value = true;

        var fPlayer = FPlayer.FromPlayer(player);
        _pedBehaviorService.StartBehavior(fPlayer, ped);
        Debug.WriteLine($"Player with net ID of {player.Character.NetworkId} alerted ped with net ID of {ped.NetworkId}.");
    }

    private void OnVehicleAlerted([FromSource] Player player, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<OnVehicleAlertedDTO>(serializedRequest);
        if (request is null)
        {
            return;
        }

        var fVehicle = this._vehicleStorageService.TryGetVehicle(request.VehicleNetworkId);
        if (fVehicle is null || fVehicle.Stopped.Value)
        {
            return;
        }

        fVehicle.Stopped.Value = true;

        foreach (var pedNetworkId in request.OccupantNetworkIds)
        {
            var ped = this._pedStorageService.TryGetPed(pedNetworkId);
            if (ped?.GetUnderlyingPed is null)
            {
                continue;
            }

            ped.Properties.SetAlwaysKeepTask(true);
            ped.Properties.SetBlockPermanentEvents(true);
            Cfx.API.SetEntityDistanceCullingRadius(ped.GetUnderlyingPed.Handle, 10000f);
        }

        var driverPed = this._pedStorageService.TryGetPed(request.DriverPedNetworkId);
        if (driverPed is null || !driverPed.Properties.IsInVehicle)
        {
            return;
        }

        var fPlayer = FPlayer.FromPlayer(player);

        if (request.CreateTrafficStopInstance)
        {
            this._trafficStopService.GenerateTrafficStop(fPlayer.Unit, fVehicle);
        }

        _pedBehaviorService.StartBehavior(fPlayer, driverPed);

        Debug.WriteLine($"Player with net ID of {player.Character.NetworkId} alerted vehicle with net ID of {request.VehicleNetworkId}");
    }

    void IInternalInteractionService.TogglePedFollowPlayer(FPlayer player, Ped ped, bool follow)
    {
        this._eventService.TriggerEventWithoutCallback(ped.Owner, Events.Interaction.ToggleFollowPlayer, ped.NetworkId, follow);
    }
}