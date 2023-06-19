using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Server.EntityWrappers.PedWrapper;
using FivePD.Gamemode.Server.EntityWrappers.VehicleWrapper;
using FivePD.Gamemode.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Models.AssistanceServiceModels;

#pragma warning disable

public class TowTruck : AssistanceService
{
    private readonly IEntityService _entityService;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;
    private readonly ICfxService _cfxService;

    private FPed _driver;
    private FVehicle _towTruck;
    private FVehicle _targetVehicle;

    private Vector3 _spawnLocation = new Vector3(704.09f, -117.47f, 52.83f);

    public TowTruck(IEntityService entityService, IConfigService configService, IRandomService randomService, ICfxService cfxService)
    {
        this._entityService = entityService;
        this._configService = configService;
        this._randomService = randomService;
        this._cfxService = cfxService;
    }

    public override async Task Setup()
    {
        this._targetVehicle = await this._cfxService.GetClosestVehicle(this.RequestedBy, this.RequestedBy.GetUnderlyingPlayer().Character.Position, 20f);
    }

    public override async Task OnCalledAsync(CancellationTokenSource cancellationTokenSource)
    {
        var serviceVehicle =
            this._configService.VehiclesConfig.TowTruck[this._randomService.Random.Next(0, this._configService.VehiclesConfig.TowTruck.Count)];

        this._driver = await this._entityService.SpawnPedAsync((uint)FPedHash.Benny, this._spawnLocation, 0f);
        this._towTruck = await this._entityService.SpawnVehicleAsync((uint)Cfx.API.GetHashKey(serviceVehicle.Model), this._spawnLocation, 0f);

        this._towTruck.Properties.CreateBlip(FBlipSprite.TowTruck, FBlipColor.TrevorOrange, "Tow truck");

        if (serviceVehicle.SirenActive)
        {
            this._towTruck.Properties.IsSirenActive(true);
        }

        this._driver.Properties.AlwaysKeepTask(true);
        this._driver.Properties.SetIntoVehicle(this._towTruck.GetUnderlyingVehicle, FVehicleSeat.Driver);
        this._driver.Properties.BlockPermanentEvents(true);
        this._driver.Task.DriveTo(
            this._towTruck.GetUnderlyingVehicle,
            this._targetVehicle.GetUnderlyingVehicle.Position,
            0f,
            25f,
            (int)FDrivingStyle.Emergency);

        while (this._towTruck.GetUnderlyingVehicle.Position.DistanceToSquared2D(this._targetVehicle.GetUnderlyingVehicle.Position) >= 150f)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                break;
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
            this.RequestedBy,
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
        this._driver.Task.CruiseWithVehicle(this._towTruck.GetUnderlyingVehicle, 12f, (int)FDrivingStyle.Normal);
        Cfx.API.SetEntityDistanceCullingRadius(this._driver.GetUnderlyingPed.NetworkId, 100f);
        Cfx.API.SetEntityDistanceCullingRadius(this._towTruck.GetUnderlyingVehicle.NetworkId, 100f);
        Cfx.API.SetEntityDistanceCullingRadius(this._targetVehicle.GetUnderlyingVehicle.NetworkId, 100f);
        this._driver.Properties.MarkAsNoLongerNeeded();
        this._towTruck.Properties.MarkAsNoLongerNeeded();
        this._targetVehicle.Properties.MarkAsNoLongerNeeded();
    }
}