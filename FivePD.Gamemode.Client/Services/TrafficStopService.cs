// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Gamemode.Client.Attributes;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class TrafficStopService : ITrafficStopService
    {
        /// <inheritdoc />
        [HoldableKeybind("fivepd_initiate_traffic_stop", "LSHIFT")]
        public void Initiate()
        {
            if (!Game.PlayerPed.IsInVehicle() || Game.PlayerPed.CurrentVehicle.Driver != Game.PlayerPed)
            {
                return;
            }

            var stoppedVehicle = this.FindClosestVehicleInFrontOfPlayer();

            // TODO: Is this range check needed?
            if (stoppedVehicle is null || stoppedVehicle.IsInRangeOf(Game.PlayerPed.Position, 25f))
            {
                return;
            }

            var trafficStop = new TrafficStop(Game.PlayerPed.NetworkId, Game.PlayerPed.Position, stoppedVehicle)
            {
                Address = Game.PlayerPed.Position.GetStreetName(),
            };

            // TODO: Send TS to server and save it.
            BaseScript.TriggerServerEvent("todo", JsonConvert.SerializeObject(trafficStop));
        }

        private Vehicle FindClosestVehicleInFrontOfPlayer()
        {
            var playerPedForwardVector = Game.PlayerPed.Position + (Game.PlayerPed.ForwardVector * 12f);
            var closestVehicleInFrontOfPlayer = World
                .GetAllVehicles()
                .Where(vehicle => !vehicle.Driver.IsPlayer)
                .Where(vehicle => vehicle.IsInRangeOf(playerPedForwardVector, 8f))
                .OrderBy(vehicle => World.GetDistance(playerPedForwardVector, vehicle.Position))
                .FirstOrDefault();

            if (closestVehicleInFrontOfPlayer is null)
            {
                return null;
            }

            if (!closestVehicleInFrontOfPlayer.IsAttached())
            {
                return closestVehicleInFrontOfPlayer;
            }

            var entityAttachedTo = closestVehicleInFrontOfPlayer.GetEntityAttachedTo();
            if (!entityAttachedTo.Model.IsVehicle)
            {
                return null;
            }

            var towingVehicle = (Vehicle)entityAttachedTo;
            return towingVehicle.Driver.IsPlayer ? null : towingVehicle;
        }
    }
}