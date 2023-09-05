using System;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Common;
using FivePD.Common.Enums;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IInteractionService" />
internal class InteractionService : IInteractionService, IServiceWithEvents
{
    private const float BaseInteractionDistance = 10.0f;
    private readonly IPedService _pedService;
    private readonly IVehicleService _vehicleService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IControlService _controlService;

    /// <summary>
    /// Initializes a new instance of
    /// the <see cref="InteractionService"/> class.
    /// </summary>
    /// <param name="pedService">The <see cref="IPedService"/> to use.</param>
    /// <param name="vehicleService">The <see cref="IVehicleService"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="controlService">The <see cref="IControlService"/> to use.</param>
    public InteractionService(
        IPedService pedService,
        IVehicleService vehicleService,
        EventHandlerDictionary eventHandlerDictionary,
        IControlService controlService)
    {
        this._pedService = pedService;
        this._vehicleService = vehicleService;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._controlService = controlService;
    }

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Interaction.GetTargetedEntity] += new Action<string, string>(this.GetTargetedEntityServer);
        this._controlService.RegisterKeybind(
            Controls.TryAlertEntityAsync.Id,
            Controls.TryAlertEntityAsync.Key,
            Controls.TryAlertEntityAsync.Title,
            this.TryAlertEntity);
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
        // Try and get the entity. If it should be a ped, then we'll
        // hint to the function to only return peds.
        var possibleEntity = GetLikelyTargetEntityInternal(typeof(T) == typeof(Ped));

        // We need to make sure the entity is of the correct type.
        // If it's not, then we'll return null.
        if (typeof(T) == typeof(Ped) && Cfx.API.IsEntityAPed(possibleEntity.Handle))
        {
            return (T)possibleEntity;
        }

        if (typeof(T) == typeof(Vehicle) && Cfx.API.IsEntityAVehicle(possibleEntity.Handle))
        {
            return (T)possibleEntity;
        }

        if (typeof(T) == typeof(Prop) && Cfx.API.IsEntityAnObject(possibleEntity.Handle))
        {
            return (T)possibleEntity;
        }

        if (typeof(T) == typeof(Pickup) && Cfx.API.IsObjectAPickup(possibleEntity.Handle))
        {
            return (T)possibleEntity;
        }

        return null;
    }

    private static Entity GetLikelyTargetEntityInternal(bool preferPeds)
    {
        // Check if we're directly targeting an entity
        // firstly, and if so, make sure they're within
        // the interaction distance. We add a bit of
        // extra distance to the interaction distance
        // because we'd expect someone directly being
        // aimed at to respond from a further range.
        var targetedEntity = TryGetEntityInAim(BaseInteractionDistance * 1.5f);

        if (targetedEntity != null)
        {
            return targetedEntity;
        }

        // Otherwise, we'll need to check if we're
        // within the interaction distance of any
        // entities by raycasting.

        // First, let's try to find entities between us and
        // the player's camera direction.
        var gameplayCamPosition = Cfx.API.GetGameplayCamCoord();
        var gameplayCamRotation = Cfx.API.GetGameplayCamRot(0);
        var cameraEntity = TryGetEntityBetweenPlayerAndPosition(
            gameplayCamPosition + (RotationToDirection(gameplayCamRotation) * BaseInteractionDistance),
            preferPeds);

        if (cameraEntity != null)
        {
            return cameraEntity;
        }

        // Nothing from camera direction?
        // Let's try to find entities between us
        // and the player's forward vector (in front
        // of us).
        var forwardEntity = TryGetEntityBetweenPlayerAndPosition(
            Game.PlayerPed.GetOffsetPosition(
                new Vector3(0.0f, BaseInteractionDistance, 0.0f)),
            preferPeds);

        if (forwardEntity != null)
        {
            return forwardEntity;
        }

        return null;
    }

    private static Entity TryGetEntityInAim(float range)
    {
        // Get the entity we're aimed at.
        var entity = Game.Player.GetTargetedEntity();

        // If we're not aiming at anything, return null.
        if (entity == null)
        {
            return null;
        }

        // If we're aiming at something, check if it's within
        // the interaction distance.
        return entity.IsInRangeOf(Game.PlayerPed.Position, range)
            ? entity
            : null;
    }

    private static Entity TryGetEntityBetweenPlayerAndPosition(Vector3 targetPosition, bool pedOnly)
    {
        var playerPosition = Game.PlayerPed.Position;

        var result = World.Raycast(
            playerPosition,
            targetPosition,
            pedOnly ? IntersectOptions.Peds1 : IntersectOptions.Everything,
            Game.PlayerPed);

        return result.DitHitEntity ? result.HitEntity : null;
    }

    private static Vector3 RotationToDirection(Vector3 rotation)
    {
        var z = rotation.Z;
        var num = z * 0.0174532924f;
        var x = rotation.X;
        var num2 = x * 0.0174532924f;
        var num3 = Math.Abs((float)Math.Cos(num2));
        return new Vector3
        {
            X = (float)(-(float)Math.Sin(num) * (double)num3),
            Y = (float)((float)Math.Cos(num) * (double)num3),
            Z = (float)Math.Sin(num2),
        };
    }

#pragma warning disable
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
        var ped = this.GetLikelyTargetEntity<Ped>();
        if (ped is null)
        {
            return;
        }

        if (ped.IsDead)
        {
            return;
        }

        ped.CreateBlip(FBlipSprite.Friend, FBlipColor.LightYellow, "Stopped ped");

        // TODO: prevent multiple alerts - statebag?
        this._pedService.GeneratePed(ped);
        BaseScript.TriggerServerEvent(Events.Ped.OnAlerted, ped.NetworkId);
    }
}