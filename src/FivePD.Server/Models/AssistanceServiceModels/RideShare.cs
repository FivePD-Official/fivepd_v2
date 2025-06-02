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
    private readonly IGameNotificationService _gameNotificationService;

    private Vector3 _target = Vector3.Zero;

    private FPed _driver;
    private FVehicle _vehicle;
    private bool _requestedForPlayer;
    private FPed _closestPed;
    private Vector3 _selectedWaypointToDrive = Vector3.Zero;

    private const float SlowingDistance = 125f;
    private const float MaximumSpeed = 30f;
    private const float MinimumSpeed = 5f;

    private readonly Guid _waypointId = Guid.NewGuid();
    private readonly string _waypointSelectedEventName;

    public RideShare(
        IEntityService entityService,
        IVehicleConfigService vehicleConfigService,
        IRandomService randomService,
        IConfirmationService confirmationService,
        EventHandlerDictionary eventHandlerDictionary,
        IGameNotificationService gameNotificationService)
    {
        this._entityService = entityService;
        this._vehicleConfigService = vehicleConfigService;
        this._randomService = randomService;
        this._confirmationService = confirmationService;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._gameNotificationService = gameNotificationService;

        this._waypointSelectedEventName = $"{Common.Events.RideShare.WaypointSelected}{this._waypointId}";
        this._eventHandlerDictionary[this._waypointSelectedEventName] += this.SetSelectedWaypointToDrive;
    }

    public override async Task SetupAsync(CancellationTokenSource cancellationTokenSource)
    {
        this._requestedForPlayer = await this._confirmationService.TriggerConfirmationAsync(this.RequestedBy, new ConfirmationProperties
        {
            Title = "Ride share",
            Message = "Who is the ride for?",
            Keys = new Dictionary<string, string>
            {
                ["F1"] = "You",
                ["F2"] = "Closest pedestrian",
            }
        });

        if (this._requestedForPlayer)
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

        await TaskHelpers.DelayUntilAsync(
            () => this._vehicle.GetUnderlyingVehicle.Position.DistanceToSquared(this._target) <= 60f,
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMilliseconds(500),
            cancellationTokenSource);

        if (cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        if (this._requestedForPlayer)
        {
            this._gameNotificationService.Notify(
                this.RequestedBy.GetUnderlyingPlayer(),
                new GameNotificationOptions
                {
                    Title = "Ride share",
                    Message = "Your ride has arrived. Please enter the vehicle and select your destination within 60 seconds.",
                });

            var waypointSelected = await TaskHelpers.DelayUntilAsync(
                () => this._selectedWaypointToDrive != Vector3.Zero,
                TimeSpan.FromSeconds(60),
                TimeSpan.Zero,
                cancellationTokenSource);
            if (!waypointSelected)
            {
                cancellationTokenSource.Cancel();
                return;
            }

            this._driver.Task.DriveTo(this._vehicle.GetUnderlyingVehicle, this._selectedWaypointToDrive, 5f, MaximumSpeed, (int)FDrivingStyle.Emergency);
            await TaskHelpers.DelayUntilAsync(
                () => this._vehicle.GetUnderlyingVehicle.Position.DistanceToSquared(this._selectedWaypointToDrive) <= 60f,
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMilliseconds(500),
                cancellationTokenSource);

            await Task.Delay(1000);

            this._gameNotificationService.Notify(
                this.RequestedBy.GetUnderlyingPlayer(),
                new GameNotificationOptions
                {
                    Title = "Ride share",
                    Message = "You have arrived at your destination. The driver will depart shortly.",
                });

            await Task.Delay(60000);
            // await TaskHelpers.DelayUntilAsync(
            //     () => this.RequestedBy.FPed.Properties.IsInVehicle, // this does not seem to work or idk, did not test
            //     TimeSpan.FromSeconds(60),
            //     TimeSpan.FromMilliseconds(500),
            //     cancellationTokenSource);
        }
        else
        {
            this._closestPed.Task.EnterVehicle(this._vehicle.GetUnderlyingVehicle, FVehicleSeat.Passenger);
            await Task.Delay(2000);
        }
    }

    public override Task OnCancellationRequestedAsync()
    {
        var entitiesToClear = new Dictionary<FEntity, FEntity>
        {
            { this._driver, this._vehicle },
            { this._vehicle, null },
        };
        if (this._closestPed is not null)
        {
            entitiesToClear[this._closestPed] = null;
        }

        this.ClearEntities(entitiesToClear);
        return Task.FromResult(0);
    }

    public override Task OnCompletedAsync()
    {
        var entitiesToClear = new Dictionary<FEntity, FEntity>
        {
            { this._driver, this._vehicle },
            { this._vehicle, null },
        };
        if (this._closestPed is not null)
        {
            entitiesToClear[this._closestPed] = null;
        }

        this.ClearEntities(entitiesToClear);
        return Task.FromResult(0);
    }

    private void SetSelectedWaypointToDrive(string serializedWaypoint)
    {
        this._selectedWaypointToDrive = JsonConvert.DeserializeObject<Vector3>(serializedWaypoint);
        this._eventHandlerDictionary[this._waypointSelectedEventName] -= this.SetSelectedWaypointToDrive;
    }
}