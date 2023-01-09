// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Utilities;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class InteractionService : IInteractionService
    {
        private const float BaseInteractionDistance = 10.0f;
        private readonly IControlService _controlService;
        private readonly IDevUtilitiesService _devUtilitiesService;
        private readonly IPedService _pedService;

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="InteractionService"/> class.
        /// </summary>
        /// <param name="controlService">The <see cref="IControlService"/> to use.</param>
        /// <param name="devUtilitiesService">The <see cref="IDevUtilitiesService"/> to use.</param>
        /// <param name="pedService">The <see cref="IPedService"/> to use.</param>
        public InteractionService(
            IControlService controlService,
            IDevUtilitiesService devUtilitiesService,
            IPedService pedService)
        {
            this._controlService = controlService;
            this._devUtilitiesService = devUtilitiesService;
            this._pedService = pedService;
        }

        /// <inheritdoc />
        public void RegisterKeybinds()
        {
            // Registers interaction keybind
            this._controlService.RegisterMomentaryKeybind(
                KeybindIdentifiers.Interact,
                "Interact with world",
                "E",
                500,
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

        /// <inheritdoc />
        public void TryAlertEntity()
        {
            // Let's try and get a ped first.
            var ped = this.GetLikelyTargetEntity<Ped>();

            if (ped != null)
            {
                if (ped.IsDead)
                {
                    // TODO
                    return;
                }

                if (ped.IsInVehicle())
                {
                    // Targeting a ped in a vehicle.
                    this._pedService.PedLeaveVehicleWithHandsUp(ped);
                    return;
                }

                this._pedService.StopPed(ped);
            }

            // Let's try and get a vehicle.
            var vehicle = this.GetLikelyTargetEntity<Vehicle>();

            if (vehicle != null)
            {
                // Targeting a vehicle.
                // TODO: Implement this.
            }
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
    }
}