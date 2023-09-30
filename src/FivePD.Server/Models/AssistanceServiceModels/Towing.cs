using System;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Models.AssistanceServiceModels;

#pragma warning disable

public class Towing : AssistanceService
{
    private readonly IEntityService _entityService;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;
    private readonly ICfxService _cfxService;
    private readonly INotificationService _notificationService;

    private FPed _driver;
    private FVehicle _towTruck;
    private FVehicle _targetVehicle;

    public Towing(
        IEntityService entityService,
        IConfigService configService,
        IRandomService randomService,
        ICfxService cfxService,
        INotificationService notificationService)
    {
        this._entityService = entityService;
        this._configService = configService;
        this._randomService = randomService;
        this._cfxService = cfxService;
        this._notificationService = notificationService;
    }

    public override async Task SetupAsync()
    {
        this._targetVehicle = this._entityService.GetClosestFVehicleToEntity(this.RequestedBy.GetUnderlyingPlayer().Character);
    }

    public override Task<bool> CanRunAsync()
    {
        if (this._targetVehicle is not null)
        {
            return Task.FromResult(true);
        }

        this._notificationService.ShowNotification(this.RequestedBy, "No vehicle found to tow.");
        return Task.FromResult(false);
    }

    public override async Task OnCalledAsync(CancellationTokenSource cancellationTokenSource)
    {
        var serviceVehicle =
            this._configService.VehiclesConfig.TowTruck[this._randomService.Random.Next(0, this._configService.VehiclesConfig.TowTruck.Count)];

        this._driver = await this._entityService.SpawnPedAsync((uint)FPedHash.Benny, this.SpawnLocation, 0f);
        this._towTruck = await this._entityService.SpawnVehicleAsync((uint)Cfx.API.GetHashKey(serviceVehicle.Model), this.SpawnLocation, 0f);

        this._towTruck.Properties.CreateBlip(FBlipSprite.TowTruck, FBlipColor.TrevorOrange, "Tow truck");

        if (serviceVehicle.SirenActive)
        {
            this._towTruck.Properties.IsSirenActive(true);
        }

        this._driver.Properties.SetAlwaysKeepTask(true);
        this._driver.Properties.SetIntoVehicle(this._towTruck.GetUnderlyingVehicle, FVehicleSeat.Driver);
        this._driver.Properties.SetBlockPermanentEvents(true);
        this._driver.Task.DriveTo(
            this._towTruck.GetUnderlyingVehicle,
            this._targetVehicle.GetUnderlyingVehicle.Position,
            0f,
            25f,
            (int)FDrivingStyle.Emergency);

        while (this._towTruck.GetUnderlyingVehicle.Position.DistanceToSquared(this._targetVehicle.GetUnderlyingVehicle.Position) >= 150f)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            await Task.Delay(100);
        }

        if (this._targetVehicle.GetUnderlyingVehicle is null)
        {
            cancellationTokenSource.Cancel();
            return;
        }

        await Task.Delay(2000);

        this._cfxService.AttachEntityToEntity(
            this._targetVehicle.GetUnderlyingVehicle.NetworkId,
            this._towTruck.GetUnderlyingVehicle.NetworkId,
            0,
            serviceVehicle.TowedVehiclePositionOffset.X,
            serviceVehicle.TowedVehiclePositionOffset.Y,
            serviceVehicle.TowedVehiclePositionOffset.Z,
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
        this._towTruck.Properties.RemoveBlips();
        this._driver.Task.CruiseWithVehicle(this._towTruck.GetUnderlyingVehicle, 12f, (int)FDrivingStyle.Normal);
        Cfx.API.SetEntityDistanceCullingRadius(this._driver.GetUnderlyingPed.Handle, 100f);
        Cfx.API.SetEntityDistanceCullingRadius(this._towTruck.GetUnderlyingVehicle.Handle, 100f);
        Cfx.API.SetEntityDistanceCullingRadius(this._targetVehicle.GetUnderlyingVehicle.Handle, 100f);
        this._driver.Properties.MarkAsNoLongerNeeded();
        this._towTruck.Properties.MarkAsNoLongerNeeded();
        this._targetVehicle.Properties.MarkAsNoLongerNeeded();
    }
}