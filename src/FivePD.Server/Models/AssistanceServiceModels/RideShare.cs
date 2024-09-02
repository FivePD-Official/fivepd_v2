using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Utilities;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Models.AssistanceServiceModels;

#pragma warning disable

public class RideShare : AssistanceService
{
    private readonly IEntityService _entityService;
    private readonly IVehicleConfigService _vehicleConfigService;
    private readonly IRandomService _randomService;
    private readonly IConfirmationService _confirmationService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly INotificationService _notificationService;

    private Vector3 _target = Vector3.Zero;

    private FPed _driver;
    private FVehicle _vehicle;
    private bool _calledForPlayer;
    private FPed _closestPed;

    private const float SlowingDistance = 125f;
    private const float MaximumSpeed = 30f;
    private const float MinimumSpeed = 5f;

    private readonly Guid _waypointId = Guid.NewGuid();

    public RideShare(
        IEntityService entityService,
        IVehicleConfigService vehicleConfigService,
        IRandomService randomService,
        IConfirmationService confirmationService,
        EventHandlerDictionary eventHandlerDictionary,
        INotificationService notificationService)
    {
        this._entityService = entityService;
        this._vehicleConfigService = vehicleConfigService;
        this._randomService = randomService;
        this._confirmationService = confirmationService;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._notificationService = notificationService;
    }

    public override async Task SetupAsync(CancellationTokenSource cancellationTokenSource)
    {
        this._calledForPlayer = await this._confirmationService.TriggerConfirmationAsync(this.RequestedBy, new ConfirmationProperties
        {
            Notification = new ConfirmationProperties.NotificationProperties
            {
                Sender = "Ride share",
                Subject = "subject",
                Content = "F1 - yourself, F2 - closest ped",
                Texture = "???",
            },
        });

        if (this._calledForPlayer)
        {
            this._target = this.RequestedBy.GetUnderlyingPlayer().Character.Position;
        }
        else
        {
            var closestPed = this._entityService.GetClosestPedToEntity(this.RequestedBy.GetUnderlyingPlayer().Character);
            if (closestPed is null)
            {
                cancellationTokenSource.Cancel();
                return;
            }

            this._closestPed = FPed.FromPed(closestPed);
            this._target = closestPed.Position;
        }
    }

    public override Task<bool> CanRunAsync()
    {
        return Task.FromResult(true);
    }

    public override async Task OnCalledAsync(CancellationTokenSource cancellationTokenSource)
    {
        var vehicles = this._vehicleConfigService.GetAll(VehicleConfigType.RideShare);
        if (vehicles.Count == 0)
        {
            vehicles.Add(DefaultVehicleConfigs.RideShare);
        }

        var serviceVehicle = vehicles[this._randomService.Random.Next(0, vehicles.Count)];

        this._driver = await this._entityService.SpawnPedAsync((uint)FPedHash.Paramedic01SMM, this.SpawnLocation, 0f, true);
        this._vehicle = await this._entityService.SpawnVehicleAsync((uint)Cfx.API.GetHashKey(serviceVehicle.Model), this.SpawnLocation, this.SpawnHeading);

        this._vehicle.GetStateBag()[StateBagKeys.Vehicle.Miscellaneous.RideShare] = this._waypointId.ToString();
        this._vehicle.Properties.CreateBlip(FBlipSprite.Cab, FBlipColor.Orange, "Ride share");

        this._driver.Properties.SetAlwaysKeepTask(true);
        this._driver.Properties.SetIntoVehicle(this._vehicle.GetUnderlyingVehicle, FVehicleSeat.Driver);
        this._driver.Properties.SetBlockPermanentEvents(true);

        this._driver.Task.DriveTo(this._vehicle.GetUnderlyingVehicle, this._target, 5f, MaximumSpeed, (int)FDrivingStyle.Emergency);

        while (this._vehicle.GetUnderlyingVehicle.Position.DistanceToSquared(this._target) > 60f)
        {
            Debug.WriteLine($"aa: {this._vehicle.GetUnderlyingVehicle.Position.DistanceToSquared(this._target)}");
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            // var distance = Vector3.Distance(this._vehicle.GetUnderlyingVehicle.Position, this._target);
            // var distanceRatio = MathHelper.Clamp((distance - SlowingDistance) / (MaximumSpeed - SlowingDistance), 0f, 1f);
            // var currentSpeed = MaximumSpeed - ((MaximumSpeed - MinimumSpeed) * distanceRatio);
            //
            // this._driver.Properties.SetDrivingSpeed(currentSpeed);

            await Task.Delay(500);
        }

        var _locationToGo = Vector3.Zero;
        if (this._calledForPlayer)
        {
            this._notificationService.ShowNotificationWithTexture(
                this.RequestedBy.GetUnderlyingPlayer(),
                "-",
                "Ride share",
                "Subject",
                "The cab has arrived. You have 60 seconds to get in and selection a location.");
            
            var eventName = $"rideshare:waypoint:selected::{this._waypointId}";

            void SetTarget(string serializedWaypoint)
            {
                Debug.WriteLine($"serializedWaypoint: {serializedWaypoint}");
                _locationToGo = JsonConvert.DeserializeObject<Vector3>(serializedWaypoint);
                this._eventHandlerDictionary[eventName] -= SetTarget;
            }

            this._eventHandlerDictionary[eventName] += SetTarget;

            var waypointSelected = await TaskHelpers.DelayUntilAsync(
                () => _locationToGo != Vector3.Zero,
                TimeSpan.FromSeconds(60),
                default,
                cancellationTokenSource);
            if (!waypointSelected)
            {
                cancellationTokenSource.Cancel();
                return;
            }

            this._driver.Task.DriveTo(this._vehicle.GetUnderlyingVehicle, _locationToGo, 5f, MaximumSpeed, (int)FDrivingStyle.Emergency);
            while (this._vehicle.GetUnderlyingVehicle.Position.DistanceToSquared(_locationToGo) > 60f)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                // var distance = Vector3.Distance(this._vehicle.GetUnderlyingVehicle.Position, this._target);
                // var distanceRatio = MathHelper.Clamp((distance - SlowingDistance) / (MaximumSpeed - SlowingDistance), 0f, 1f);
                // var currentSpeed = MaximumSpeed - ((MaximumSpeed - MinimumSpeed) * distanceRatio);
                //
                // this._driver.Properties.SetDrivingSpeed(currentSpeed);

                await Task.Delay(500);
            }
            await Task.Delay(1000);

            this._notificationService.ShowNotificationWithTexture(
                this.RequestedBy.GetUnderlyingPlayer(),
                "-",
                "Ride share",
                "Subject",
                "You've arrived to the selected location. The cab will drive away in 5 seconds.");
            await Task.Delay(5000);
        }
        else
        {
            this._closestPed.Task.EnterVehicle(this._vehicle.GetUnderlyingVehicle, FVehicleSeat.Passenger);
            await Task.Delay(2000);
        }
    }

    public override Task OnCancellationRequestedAsync()
    {
        this.ClearEntities(new Dictionary<FEntity, FEntity>
        {
            { this._driver, this._vehicle },
            { this._vehicle, null },
            // { this._closestPed, null },
        });
        return Task.FromResult(0);
    }

    public override Task OnCompletedAsync()
    {
        this.ClearEntities(new Dictionary<FEntity, FEntity>
        {
            { this._driver, this._vehicle },
            { this._vehicle, null },
            // { this._closestPed, null },
        });
        return Task.FromResult(0);
    }
}