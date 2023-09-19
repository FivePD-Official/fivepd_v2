using System;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc cref="FivePD.Server.Interfaces.IInteractionService" />
public class InteractionService : IInteractionService, IServiceWithEvents
{
    private readonly IPedService _pedService;
    private readonly IVehicleService _vehicleService;
    private readonly IControlService _controlService;
    private readonly IEventService _eventService;
    private readonly ITickQueueService _tickQueueService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionService"/> class.
    /// </summary>
    /// <param name="pedService">The <see cref="IPedService"/> to use.</param>
    /// <param name="vehicleService">The <see cref="IVehicleService"/> to use.</param>
    /// <param name="controlService">The <see cref="IControlService"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public InteractionService(
        IPedService pedService,
        IVehicleService vehicleService,
        IControlService controlService,
        IEventService eventService,
        ITickQueueService tickQueueService,
        EventHandlerDictionary eventHandlerDictionary)
    {
        this._pedService = pedService;
        this._vehicleService = vehicleService;
        this._controlService = controlService;
        this._eventService = eventService;
        this._tickQueueService = tickQueueService;
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc />
    void IServiceWithEvents.RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Ped.OnAlerted] += new Action<Player, int>(this.OnAlerted);
    }

    private async void OnAlerted([FromSource] Player player, int networkId)
    {
        var ped = this._pedService.TryGetPed(networkId);
        if (ped is null)
        {
            return;
        }

        ped.Stopped.Value = true;

        Cfx.API.SetEntityDistanceCullingRadius(ped.GetUnderlyingPed.Handle, 10000f);

        ped.Properties.SetAlwaysKeepTask(true);
        ped.Properties.SetBlockPermanentEvents(true);

        if (ped.Properties.IsInVehicle)
        {
            ped.Task.LeaveVehicle();
        }

        ped.Task.StandStill(-1);
        ped.Task.TurnTo(player.Character);

        Debug.WriteLine($"Alerted ped with net ID of {ped.NetworkId}");
    }
}