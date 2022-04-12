// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Gamemode.Client.Attributes;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Utilities;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class TrafficStopService : ITrafficStopService
    {
        private readonly IPoliceEventService _policeEventService;
        private readonly INotificationService _notificationService;
        private readonly IVehicleService _vehicleService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrafficStopService"/> class.
        /// </summary>
        /// <param name="policeEventService">The <see cref="IPoliceEventService"/> to use.</param>
        /// <param name="notificationService">The <see cref="INotificationService"/> to use.</param>
        /// <param name="vehicleService">The <see cref="IVehicleService"/> to use.</param>
        public TrafficStopService(IPoliceEventService policeEventService, INotificationService notificationService, IVehicleService vehicleService)
        {
            this._policeEventService = policeEventService;
            this._notificationService = notificationService;
            this._vehicleService = vehicleService;
        }

        /// <inheritdoc />
        [Keybind("fivepd_traffic_stop", "LSHIFT")]
        public async Task Initiate()
        {
            // TODO: return if the vehicle is attached to a traffic stop
            if (this._policeEventService.IsAttachedToEvent() && this._policeEventService.GetCurrentEvent() is TrafficStop)
            {
                return;
            }

            if (!Game.PlayerPed.IsInVehicle() || Game.PlayerPed.CurrentVehicle.Driver != Game.PlayerPed)
            {
                return;
            }

            var stoppedVehicle = this.FindClosestVehicleInFrontOfPlayer();
            stoppedVehicle.CreateBlip(BlipSprite.PersonalVehicleCar, FBlipColor.LemonGreen, "Pulled over vehicle");
            this._vehicleService.GenerateVehicle(stoppedVehicle);

            // TODO: Is this range check needed?
            if (stoppedVehicle is null || !stoppedVehicle.IsInRangeOf(Game.PlayerPed.Position, 25f))
            {
                return;
            }

            var trafficStop = new TrafficStop(Game.PlayerPed.NetworkId, Game.PlayerPed.Position, stoppedVehicle)
            {
                Title = "Traffic stop",
                Address = Game.PlayerPed.Position.GetStreetName(),
            };
            this._policeEventService.SetCurrentEvent(trafficStop);
            BaseScript.TriggerServerEvent(Events.TrafficStop.Initiate, JsonConvert.SerializeObject(trafficStop));

            // TODO: Use localization.
            this._notificationService.ShowTooltip("Turn on your sirens to initiate the traffic stop.", true);
            while (Game.PlayerPed.IsInVehicle() && !Game.PlayerPed.CurrentVehicle.IsSirenActive)
            {
                if (!this._policeEventService.IsAttachedToEvent())
                {
                    return;
                }

                await BaseScript.Delay(50);
            }

            // TODO: Use localization.
            this._notificationService.ShowNotification("Wait until the vehicle has come to a stop, then proceed with the traffic stop.");

            // TODO: Is this persistency thing needed with OneSync?
            stoppedVehicle.Driver.IsPersistence(true);
            foreach (var ped in stoppedVehicle.Occupants)
            {
                ped.IsPersistence(true);
            }

            stoppedVehicle.Driver.KeepTask(true);
            stoppedVehicle.Driver.Task.ClearAll();

            var stopPoint = default(Vector3);
            var stoppedVehicleForwardVector = stoppedVehicle.Position + (stoppedVehicle.ForwardVector * 40f);
            Cfx.API.GetRoadSidePointWithHeading(stoppedVehicleForwardVector.X, stoppedVehicleForwardVector.Y, stoppedVehicleForwardVector.Z, stoppedVehicle.Heading, ref stopPoint);

            stoppedVehicle.Driver.Task.DriveTo(stoppedVehicle, stopPoint, 2f, 10f);
            while (!stoppedVehicle.IsInRangeOf(stopPoint, 2f))
            {
                await BaseScript.Delay(50);
            }

            stoppedVehicle.Driver.Task.StandStill(-1);
            this._notificationService.ShowNotification("The vehicle has stopped. Proceed with the traffic stop.");
        }

        /// <inheritdoc />
        [Keybind("fivepd_traffic_stop", "LSHIFT")]
        public void Cancel()
        {
            if (!this._policeEventService.IsAttachedToEvent() || (!this._policeEventService.IsEventMainPlayerEqualsToPlayer() && this._policeEventService.GetCurrentEvent() is TrafficStop))
            {
                return;
            }

            BaseScript.TriggerServerEvent(Events.TrafficStop.Cancel, this._policeEventService.GetCurrentEvent().Id.ToString());
            this._policeEventService.SetCurrentEvent(null);
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