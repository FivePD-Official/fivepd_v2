using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Utilities;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Models.AssistanceServiceModels;

/// <summary>
/// Represents a towing assistance service.
/// </summary>
public class Towing : AssistanceService
{
    private const float SlowingDistance = 125f;
    private const float MaximumSpeed = 25f;
    private const float MinimumSpeed = 5f;

    private readonly IEntityService _entityService;
    private readonly IRandomService _randomService;
    private readonly ICfxService _cfxService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly IVehicleConfigService _vehicleConfigService;

    private FPed _driver;
    private FVehicle _towTruck;
    private FVehicle _targetVehicle;

    private Vector3 _targetPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="Towing"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="randomService">The random service.</param>
    /// <param name="cfxService">The cfx service.</param>
    /// <param name="gameNotificationService">The game notification service.</param>
    /// <param name="vehicleConfigService">The vehicle configuration service.</param>
    public Towing(
        IEntityService entityService,
        IRandomService randomService,
        ICfxService cfxService,
        IGameNotificationService gameNotificationService,
        IVehicleConfigService vehicleConfigService)
    {
        this._entityService = entityService;
        this._randomService = randomService;
        this._cfxService = cfxService;
        this._gameNotificationService = gameNotificationService;
        this._vehicleConfigService = vehicleConfigService;
    }

    /// <inheritdoc />
    public override async Task SetupAsync(CancellationTokenSource cancellationTokenSource)
    {
        this._targetVehicle = this._entityService.GetClosestFVehicleToEntity(this.RequestedBy.GetUnderlyingPlayer().Character);
        this._targetPosition = this._targetVehicle?.GetUnderlyingVehicle.Position ?? default;
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task<bool> CanRunAsync()
    {
        if (this._targetVehicle is not null)
        {
            return Task.FromResult(true);
        }

        this._gameNotificationService.Notify(
            this.RequestedBy,
            new GameNotificationOptions
            {
                Title = "FivePD",
                Message = "No vehicle found to tow.",
            });
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public override async Task OnCalledAsync(CancellationTokenSource cancellationTokenSource)
    {
        var towTrucks = this._vehicleConfigService.GetAll<TowTruckVehicleConfigOptions>(VehicleConfigType.TowTruck);
        if (towTrucks.Count == 0)
        {
            towTrucks.Add(DefaultVehicleConfigs.TowTruck);
        }

        var serviceVehicle = towTrucks[this._randomService.Random.Next(0, towTrucks.Count)];

        this._driver = await this._entityService.SpawnPedAsync((uint)FPedHash.Benny, this.SpawnLocation, 0f, true);
        this._towTruck =
            await this._entityService.SpawnVehicleAsync((uint)Cfx.API.GetHashKey(serviceVehicle.Model), this.SpawnLocation, this.SpawnHeading);

        this._towTruck.Properties.CreateBlip(FBlipSprite.TowTruck, FBlipColor.TrevorOrange, "Tow truck");

        if (serviceVehicle.Options.SirenActive)
        {
            this._towTruck.Properties.IsSirenActive(true);
        }

        this._driver.Properties.SetAlwaysKeepTask(true);
        this._driver.Properties.SetIntoVehicle(this._towTruck.GetUnderlyingVehicle, FVehicleSeat.Driver);
        this._driver.Properties.SetBlockPermanentEvents(true);

        this._driver.Task.DriveTo(this._towTruck.GetUnderlyingVehicle, this._targetPosition, 0f, MaximumSpeed, (int)FDrivingStyle.Emergency);

        while (this._towTruck.GetUnderlyingVehicle.Position.DistanceToSquared(this._targetPosition) >= SlowingDistance)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            var distance = Vector3.Distance(this._towTruck.GetUnderlyingVehicle.Position, this._targetPosition);
            var distanceRatio = MathHelper.Clamp((distance - SlowingDistance) / (MaximumSpeed - SlowingDistance), 0f, 1f);
            var currentSpeed = MaximumSpeed - ((MaximumSpeed - MinimumSpeed) * distanceRatio);

            this._driver.Properties.SetDrivingSpeed(currentSpeed);

            await Task.Delay(500);
        }

        this._driver.Task.ClearAll();

        this._driver.Task.DriveTo(
            this._towTruck.GetUnderlyingVehicle,
            this._targetPosition,
            0f,
            5f,
            (int)FDrivingStyle.Normal);

        await Task.Delay(2000);

        if (this._targetVehicle.GetUnderlyingVehicle is null || this._towTruck.GetUnderlyingVehicle.Position.DistanceToSquared(this._targetPosition) >= 150f)
        {
            cancellationTokenSource.Cancel();
            this._gameNotificationService.Notify(
                this.RequestedBy,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "Towing has been canceled. The target vehicle has moved or is missing.",
                });
            return;
        }

        this._cfxService.AttachEntityToEntity(
            this._targetVehicle.GetUnderlyingVehicle.NetworkId,
            this._towTruck.GetUnderlyingVehicle.NetworkId,
            0,
            serviceVehicle.Options.TowedVehiclePositionOffset.X,
            serviceVehicle.Options.TowedVehiclePositionOffset.Y,
            serviceVehicle.Options.TowedVehiclePositionOffset.Z,
            0f,
            0f,
            0f,
            false,
            false,
            false,
            false,
            0,
            true);
    }

    /// <inheritdoc />
    public override Task OnCancellationRequestedAsync()
    {
        return this.OnCompletedOrCanceledAsync();
    }

    /// <inheritdoc />
    public override Task OnCompletedAsync()
    {
        return this.OnCompletedOrCanceledAsync();
    }

    private Task OnCompletedOrCanceledAsync()
    {
        this.ClearEntities(new Dictionary<FEntity, FEntity>()
        {
            { this._driver, this._towTruck },
            { this._towTruck, null },
        });

        return Task.CompletedTask;
    }
}