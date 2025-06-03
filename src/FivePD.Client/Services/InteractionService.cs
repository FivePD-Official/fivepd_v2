using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IInteractionService" />
internal class InteractionService : IInteractionService, IOnInit
{
    private const float BaseInteractionDistance = 15f;
    private readonly IPedService _pedService;
    private readonly IVehicleService _vehicleService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IControlService _controlService;
    private readonly ITickService _tickService;
    private readonly ILocalPlayerService _localPlayerService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly IVehicleConfigService _vehicleConfigService;

    private Ped _currentlyFollowingPed;

    /// <summary>
    /// Initializes a new instance of
    /// the <see cref="InteractionService"/> class.
    /// </summary>
    /// <param name="pedService">The <see cref="IPedService"/> to use.</param>
    /// <param name="vehicleService">The <see cref="IVehicleService"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="controlService">The <see cref="IControlService"/> to use.</param>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    /// <param name="localPlayerService">The <see cref="ILocalPlayerService"/> to use.</param>
    /// <param name="gameNotificationService">The <see cref="IGameNotificationService"/> to use.</param>
    /// <param name="vehicleConfigService">The <see cref="IVehicleConfigService"/> to use.</param>
    public InteractionService(
        IPedService pedService,
        IVehicleService vehicleService,
        EventHandlerDictionary eventHandlerDictionary,
        IControlService controlService,
        ITickService tickService,
        ILocalPlayerService localPlayerService,
        IGameNotificationService gameNotificationService,
        IVehicleConfigService vehicleConfigService)
    {
        this._pedService = pedService;
        this._vehicleService = vehicleService;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._controlService = controlService;
        this._tickService = tickService;
        this._localPlayerService = localPlayerService;
        this._gameNotificationService = gameNotificationService;
        this._vehicleConfigService = vehicleConfigService;
    }

    /// <inheritdoc />
    public void OnInit()
    {
        this._eventHandlerDictionary[Events.Interaction.GetTargetedEntity] += new Action<string, string>(this.GetTargetedEntityServer);
        this._eventHandlerDictionary[Events.Interaction.ToggleFollowPlayer] += new Action<int, bool>(this.ToggleFollowPlayer);

        this._controlService.RegisterMomentaryKeybind(
            Controls.TryAlertEntity.Id,
            Controls.TryAlertEntity.Key,
            Controls.TryAlertEntity.Title,
            Controls.TryAlertEntity.TimerBarTitle,
            Controls.TryAlertEntity.TimeToHoldInMs,
            this.TryAlertEntity,
            canInvokeOnPress: () => this._localPlayerService.Player.DutyStatus.Value == FDutyStatus.OnDuty && !Game.PlayerPed.IsInVehicle());

        this._controlService.RegisterMomentaryKeybind(
            Controls.TryAlertVehicle.Id,
            Controls.TryAlertVehicle.Key,
            Controls.TryAlertVehicle.Title,
            Controls.TryAlertVehicle.TimerBarTitle,
            Controls.TryAlertVehicle.TimeToHoldInMs,
            this.TryAlertVehicle,
            canInvokeOnPress: () => this._localPlayerService.Player.DutyStatus.Value == FDutyStatus.OnDuty && Game.PlayerPed.IsInVehicle() && this._vehicleConfigService.IsPoliceVehicle(Game.PlayerPed.CurrentVehicle));

        this._controlService.RegisterMomentaryKeybind(
            Controls.EnterVehicleAsPassenger.Id,
            Controls.EnterVehicleAsPassenger.Key,
            Controls.EnterVehicleAsPassenger.Title,
            Controls.EnterVehicleAsPassenger.TimerBarTitle,
            Controls.EnterVehicleAsPassenger.TimeToHoldInMs,
            this.EnterVehicleAsPassenger,
            canInvokeOnPress: () => !Game.PlayerPed.IsInVehicle());
    }

    /// <inheritdoc/>
    public Entity GetLikelyTargetEntity()
    {
        return GetLikelyTargetEntityInternal(false);
    }

    /// <inheritdoc/>
    public T GetLikelyTargetEntity<T>()
        where T : Entity
    {
        var possibleEntity = GetLikelyTargetEntityInternal(typeof(T) == typeof(Ped));
        if (possibleEntity is null)
        {
            return null;
        }

        if (typeof(T) == typeof(Ped) && Cfx.API.IsEntityAPed(possibleEntity.Handle))
        {
            return (T)possibleEntity;
        }

        if (typeof(T) == typeof(Vehicle) && Cfx.API.IsEntityAVehicle(possibleEntity.Handle))
        {
            return (T)possibleEntity;
        }

#pragma warning disable
        // if (typeof(T) == typeof(Prop) && Cfx.API.IsEntityAnObject(possibleEntity.Handle))
        // {
        //     return (T)possibleEntity;
        // }
        //
        // if (typeof(T) == typeof(Pickup) && Cfx.API.IsObjectAPickup(possibleEntity.Handle))
        // {
        //     return (T)possibleEntity;
        // }
#pragma warning restore

        return null;
    }

    private static Entity GetLikelyTargetEntityInternal(bool preferPeds)
    {
        var targetedEntity = TryGetEntityInAim(BaseInteractionDistance * 1.5f);
        if (targetedEntity is not null)
        {
            return targetedEntity;
        }

        IEnumerable<Entity> entities;
        if (preferPeds)
        {
            entities = World.GetAllPeds();
        }
        else
        {
            entities = World.GetAllVehicles();
        }

        var closestEntity = entities
            .OrderBy(x => World.GetDistance(x.Position, Game.PlayerPed.Position))
            .FirstOrDefault(x =>
                Cfx.API.NetworkGetEntityIsNetworked(x.Handle) && x.NetworkId != Game.PlayerPed.NetworkId &&
                x.IsInRangeOf(Game.PlayerPed.Position, BaseInteractionDistance));

        return closestEntity;
    }

    private static Entity TryGetEntityInAim(float range)
    {
        var entity = Game.Player.GetTargetedEntity();
        if (entity is null)
        {
            return null;
        }

        return entity.IsInRangeOf(Game.PlayerPed.Position, range)
            ? entity
            : null;
    }

    // TODO: remove?
    private void GetTargetedEntityServer(string eventId, string type)
    {
        var possibleEntity = GetLikelyTargetEntityInternal(type == "ped");
        if (possibleEntity is null)
        {
            BaseScript.TriggerServerEvent(Events.Generic.Callback, eventId, "-1");
            return;
        }

        // We need to make sure the entity is of the correct type.
        // If it's not, then we'll return null.
        if (type == "ped" && Cfx.API.IsEntityAPed(possibleEntity.Handle))
        {
            this._pedService.GeneratePed((Ped)possibleEntity);
            BaseScript.TriggerServerEvent(Events.Generic.Callback, eventId, possibleEntity.NetworkId.ToString());
            return;
        }

        // TODO: seems not to be working
        if (type == "vehicle" && Cfx.API.IsEntityAVehicle(possibleEntity.Handle))
        {
            this._vehicleService.GenerateVehicle((Vehicle)possibleEntity);
            BaseScript.TriggerServerEvent(Events.Generic.Callback, eventId, possibleEntity.NetworkId.ToString());
            return;
        }

        BaseScript.TriggerServerEvent(Events.Generic.Callback, eventId, "-1");
    }

    private void TryAlertEntity()
    {
        if (this._localPlayerService.Player.DutyStatus.Value == FDutyStatus.OffDuty)
        {
            return;
        }

        if (Game.PlayerPed.IsInVehicle())
        {
            return;
        }

        var ped = this.GetLikelyTargetEntity<Ped>();
        if (ped is null)
        {
            return;
        }

        if (ped.IsDead || ped.IsInVehicle())
        {
            return;
        }

        var fPed = this._pedService.GeneratePed(ped);
        if (fPed is null || fPed.Stopped.Value)
        {
            return;
        }

        ped.CreateBlip(FBlipSprite.Friend, FBlipColor.LightYellow, "Stopped ped");
        BaseScript.TriggerServerEvent(Events.Ped.OnAlerted, ped.NetworkId);
    }

#pragma warning disable

    private void TryAlertVehicle()
    {
        if (this._localPlayerService.Player.DutyStatus.Value == FDutyStatus.OffDuty)
        {
            return;
        }

        if (this._localPlayerService.Information.IncidentWrapper is not null &&
            this._localPlayerService.Information.IncidentWrapper.Incident.Type == FIncidentType.TrafficStop)
        {
            return;
        }

        // Handle on foot
        if (!Game.PlayerPed.IsInVehicle())
        {
            var closestVehicle = World.GetAllVehicles()
                .OrderBy(x => World.GetDistance(x.Position, Game.PlayerPed.Position))
                .FirstOrDefault(x =>
                    Cfx.API.NetworkGetEntityIsNetworked(x.Handle) && x.NetworkId != Game.PlayerPed.NetworkId &&
                    x.IsInRangeOf(Game.PlayerPed.Position, BaseInteractionDistance));
            if (closestVehicle?.Driver is null)
            {
                return;
            }

            var fVehicle = this._vehicleService.GenerateVehicle(closestVehicle);
            if (fVehicle is null || fVehicle.Stopped.Value)
            {
                return;
            }

            BaseScript.TriggerServerEvent(Events.Vehicle.OnAlerted, JsonConvert.SerializeObject(new OnVehicleAlertedDTO
            {
                AlertedOnFoot = true,
                VehicleNetworkId = closestVehicle.NetworkId,
                OccupantNetworkIds = closestVehicle.Occupants.Where(x => Cfx.API.NetworkGetEntityIsNetworked(x.Handle)).Select(x => x.NetworkId).ToList(),
                DriverPedNetworkId = closestVehicle.Driver.NetworkId,
                CreateTrafficStopInstance = this._localPlayerService.Information.IncidentWrapper is null,
            }));
            return;
        }

        // Handle in vehicle
        if (Game.PlayerPed.CurrentVehicle is not null && Game.PlayerPed.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) != Game.PlayerPed)
        {
            return;
        }
    }

    private void EnterVehicleAsPassenger()
    {
        var closestVehicle = World.GetAllVehicles()
            .OrderBy(x => World.GetDistance(x.Position, Game.PlayerPed.Position))
            .FirstOrDefault(x =>
                Cfx.API.NetworkGetEntityIsNetworked(x.Handle) && x.NetworkId != Game.PlayerPed.NetworkId &&
                x.IsInRangeOf(Game.PlayerPed.Position, BaseInteractionDistance));

        if (closestVehicle is null)
        {
            return;
        }

        var preferredPassengerSeats = new List<VehicleSeat>
        {
            VehicleSeat.LeftRear,
            VehicleSeat.RightRear,
            VehicleSeat.ExtraSeat1,
            VehicleSeat.ExtraSeat2,
            VehicleSeat.ExtraSeat3,
            VehicleSeat.ExtraSeat4,
            VehicleSeat.ExtraSeat5,
            VehicleSeat.ExtraSeat6,
            VehicleSeat.ExtraSeat7,
            VehicleSeat.ExtraSeat8,
            VehicleSeat.ExtraSeat9,
            VehicleSeat.ExtraSeat10,
            VehicleSeat.ExtraSeat11,
            VehicleSeat.ExtraSeat12,
        };

        var seatToEnter = VehicleSeat.Any;
        foreach (var seat in preferredPassengerSeats)
        {
            if (closestVehicle.IsSeatFree(seat))
            {
                seatToEnter = seat;
                break;
            }
        }

        if (seatToEnter == VehicleSeat.Any && closestVehicle.IsSeatFree(VehicleSeat.Passenger))
        {
            seatToEnter = VehicleSeat.Passenger;
        }

        if (seatToEnter != VehicleSeat.Any)
        {
            Game.PlayerPed.Task.EnterVehicle(closestVehicle, seatToEnter);
            return;
        }

        this._gameNotificationService.Notify(new GameNotificationOptions
        {
            Title = "FivePD",
            Message = "No free passenger seats available in this vehicle.",
        });
    }

    private void ToggleFollowPlayer(int networkId, bool follow)
    {
        if (this._currentlyFollowingPed is not null && this._currentlyFollowingPed.NetworkId == networkId)
        {
            this._currentlyFollowingPed = null;
            this._tickService.OnTick -= this.FollowAsync;
            return;
        }

        if (!Cfx.API.NetworkDoesEntityExistWithNetworkId(networkId))
        {
            return;
        }

        if (Entity.FromNetworkId(networkId) is not Ped ped)
        {
            return;
        }

        this._currentlyFollowingPed = ped;

        if (follow)
        {
            this._tickService.OnTick += this.FollowAsync;
        }
        else
        {
            this._tickService.OnTick -= this.FollowAsync;
        }
    }

    private async Task FollowAsync()
    {
        if (this._currentlyFollowingPed is not null)
        {
            if (this._currentlyFollowingPed.IsInRangeOf(Game.PlayerPed.Position, 3f))
            {
                this._currentlyFollowingPed.Task.ChatTo(Game.PlayerPed);
            }
            else
            {
                this._currentlyFollowingPed.Task.RunTo(Game.PlayerPed.Position);
            }

            await BaseScript.Delay(200);
        }
    }
}
