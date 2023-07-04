using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Enums;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionService"/> class.
    /// </summary>
    /// <param name="pedService">The <see cref="IPedService"/> to use.</param>
    /// <param name="vehicleService">The <see cref="IVehicleService"/> to use.</param>
    /// <param name="controlService">The <see cref="IControlService"/> to use.</param>
    public InteractionService(
        IPedService pedService,
        IVehicleService vehicleService,
        IControlService controlService,
        IEventService eventService,
        ITickQueueService tickQueueService)
    {
        this._pedService = pedService;
        this._vehicleService = vehicleService;
        this._controlService = controlService;
        this._eventService = eventService;
        this._tickQueueService = tickQueueService;
    }

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this._controlService.RegisterKeybind(
            Controls.TryAlertEntityAsync.Id,
            Controls.TryAlertEntityAsync.Key,
            Controls.TryAlertEntityAsync.Title,
            player => this._tickQueueService.EnqueueAsync(() => this.TryAlertEntityAsync(player)));
    }

    /// <inheritdoc />
    public async Task TryAlertEntityAsync(Player player)
    {
        Debug.WriteLine("invoked: TryAlertEntityAsync");
        // TODO: return if player is in vehicle

        var ped = await this.GetTargetedEntityAsync<FPed>(player);
        if (ped is null)
        {
            return;
        }

        if (await ped.Properties.IsDead)
        {
            // TODO
            return;
        }

        if (await ped.Properties.IsInVehicle)
        {
            // Targeting a ped in a vehicle.
            // TODO this._pedService.PedLeaveVehicleWithHandsUp(ped);
            return;
        }

        // TODO this._pedService.StopPed(ped);
        ped.Properties.CreateBlip(FBlipSprite.Friend, FBlipColor.LightYellow, "Stopped ped");

        ped.Properties.AlwaysKeepTask(true);
        ped.Properties.BlockPermanentEvents(true);
        ped.Task.StandStill(-1);
        ped.Task.TurnTo(player.Character);

        Debug.WriteLine($"Alerted ped with net ID of {ped.GetFPed.NetworkId}");
    }

#pragma warning disable
    public async Task<T> GetTargetedEntityAsync<T>(Player player)
        where T : FEntity
    {
        var type = string.Empty;
        if (typeof(T) == typeof(FPed))
        {
            type = "ped";
        }
        else if (typeof(T) == typeof(FVehicle))
        {
            type = "vehicle";
        }

        var networkId = await this._eventService.TriggerEventAsync(player, Events.Interaction.GetTargetedEntity, type);
        if (networkId == "-1")
        {
            return null;
        }

        if (typeof(T) == typeof(FPed))
        {
            var fPed = this._pedService.TryGetPed(int.Parse(networkId));
            return fPed as T;
        }

        if (typeof(T) == typeof(FVehicle))
        {
            var fVehicle = this._vehicleService.TryGetVehicle(int.Parse(networkId));
            return fVehicle as T;
        }

        return null;
    }
}