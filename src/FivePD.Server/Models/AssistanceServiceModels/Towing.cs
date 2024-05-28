using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Utilities;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Models.AssistanceServiceModels;

#pragma warning disable

public class Towing : AssistanceService
{
    private readonly IEntityService _entityService;
    private readonly IRandomService _randomService;
    private readonly ICfxService _cfxService;
    private readonly INotificationService _notificationService;
    private readonly IVehicleConfigService _vehicleConfigService;

    private FPed _driver;
    private FVehicle _towTruck;
    private FVehicle _targetVehicle;

    private const float SlowingDistance = 125f;
    private const float MaximumSpeed = 25f;
    private const float MinimumSpeed = 5f;

    private Vector3 _targetPosition;

    public Towing(
        IEntityService entityService,
        IRandomService randomService,
        ICfxService cfxService,
        INotificationService notificationService,
        IVehicleConfigService vehicleConfigService)
    {
        this._entityService = entityService;
        this._randomService = randomService;
        this._cfxService = cfxService;
        this._notificationService = notificationService;
        this._vehicleConfigService = vehicleConfigService;
    }

    public override async Task SetupAsync()
    {
        this._targetVehicle = this._entityService.GetClosestFVehicleToEntity(this.RequestedBy.GetUnderlyingPlayer().Character);
        this._targetPosition = this._targetVehicle?.GetUnderlyingVehicle.Position ?? default;
    }

    public override Task<bool> CanRunAsync()
    {
        if (this._targetVehicle is not null)
        {
            Debug.WriteLine("Target vehicle is not null. Returning true");
            return Task.FromResult(true);
        }

        Debug.WriteLine("Target vehicle is null. Returning false.");
        this._notificationService.ShowNotification(this.RequestedBy, "No vehicle found to tow.");
        return Task.FromResult(false);
    }

    public override async Task OnCalledAsync(CancellationTokenSource cancellationTokenSource)
    {
        var towTrucks = this._vehicleConfigService.GetAll<TowTruckVehicleConfigOptions>(VehicleConfigType.TowTruck);
        if (towTrucks.Count == 0)
        {
            towTrucks.Add(DefaultVehicleConfigs.TowTruck);
        }

        var serviceVehicle = towTrucks[this._randomService.Random.Next(0, towTrucks.Count)];

        this._driver = await this._entityService.SpawnPedAsync((uint)FPedHash.Benny, this.SpawnLocation, 0f);
        this._towTruck = await this._entityService.SpawnVehicleAsync((uint)Cfx.API.GetHashKey(serviceVehicle.Model), this.SpawnLocation, this.SpawnHeading);

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
                Debug.WriteLine("Cancelling...");
                return;
            }

            var distance = Vector3.Distance(this._towTruck.GetUnderlyingVehicle.Position, this._targetPosition);
            var distanceRatio = MathHelper.Clamp((distance - SlowingDistance) / (MaximumSpeed - SlowingDistance), 0f, 1f);
            var currentSpeed = MaximumSpeed - ((MaximumSpeed - MinimumSpeed) * distanceRatio);

            this._driver.Properties.SetMaxDrivingSpeed(currentSpeed);

            await Task.Delay(500);
        }

        this._driver.Task.ClearAll();

        if (this._targetVehicle.GetUnderlyingVehicle is null || this._towTruck.GetUnderlyingVehicle.Position.DistanceToSquared(this._targetPosition) >= 150f)
        {
            cancellationTokenSource.Cancel();
            Debug.WriteLine("Running cancel1");
            this._notificationService.ShowNotification(this.RequestedBy, "ERROR: The vehicle has moved or is missing!");
            return;
        }

        this._driver.Task.DriveTo(
            this._towTruck.GetUnderlyingVehicle,
            this._targetVehicle.GetUnderlyingVehicle.Position,
            0f,
            5f,
            (int)FDrivingStyle.Normal);

        await Task.Delay(2000);

        if (this._targetVehicle.GetUnderlyingVehicle is null || this._towTruck.GetUnderlyingVehicle.Position.DistanceToSquared(this._targetPosition) >= 150f)
        {
            cancellationTokenSource.Cancel();
            this._notificationService.ShowNotification(this.RequestedBy, "ERROR: The vehicle has moved or is missing!");
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

    public override Task OnCancellationRequestedAsync()
    {
        this.OnCompletedOrCancelled();
        return Task.FromResult(0);
    }

    public override Task OnCompletedAsync()
    {
        this.OnCompletedOrCancelled();
        return Task.FromResult(0);
    }

    private void OnCompletedOrCancelled()
    {
        if (this._driver?.GetUnderlyingPed is not null)
        {
            Debug.WriteLine("DriverCheck1");
            Cfx.API.SetEntityDistanceCullingRadius(this._driver.GetUnderlyingPed.Handle, 100f);
            Debug.WriteLine("DriverCheck2");
            this._driver.Task.CruiseWithVehicle(this._towTruck.GetUnderlyingVehicle, 12f, (int)FDrivingStyle.Normal);
            Debug.WriteLine("DriverCheck3");
            this._driver.Properties.MarkAsNoLongerNeeded();
            Debug.WriteLine("DriverCheck4");
        }

        if (this._towTruck?.GetUnderlyingVehicle is not null)
        {
            Debug.WriteLine("TowCheck1");
            this._towTruck.Properties.RemoveBlips();
            Debug.WriteLine("TowCheck2");
            Cfx.API.SetEntityDistanceCullingRadius(this._towTruck.GetUnderlyingVehicle.Handle, 100f);
            Debug.WriteLine("TowCheck3");
            this._towTruck.Properties.MarkAsNoLongerNeeded();
            Debug.WriteLine("TowCheck4");
        }

        if (this._targetVehicle?.GetUnderlyingVehicle is not null)
        {
            Debug.WriteLine("TargetCheck1");
            Cfx.API.SetEntityDistanceCullingRadius(this._targetVehicle.GetUnderlyingVehicle.Handle, 100f);
            Debug.WriteLine("TargetCheck2");
            this._targetVehicle.Properties.MarkAsNoLongerNeeded();
            Debug.WriteLine("TargetCheck3");
        }
    }
}