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

public class Ambulance : AssistanceService
{
    private readonly IEntityService _entityService;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;

    private FPed _driver;
    private FVehicle _ambulance;

    private Vector3 _spawnLocation = new Vector3(704.09f, -117.47f, 52.83f);
    private Vector3 _target;

    public Ambulance(IEntityService entityService, IConfigService configService, IRandomService randomService)
    {
        this._entityService = entityService;
        this._configService = configService;
        this._randomService = randomService;
    }

    public override async Task Setup()
    {
        this._target = this.RequestedBy.GetUnderlyingPlayer().Character.Position;
    }
    
    public override async Task OnCalledAsync(CancellationTokenSource cancellationTokenSource)
    {
        var serviceVehicle = this._configService.VehiclesConfig.Ambulance[this._randomService.Random.Next(0, this._configService.VehiclesConfig.Ambulance.Count)];

        this._driver = await this._entityService.SpawnPedAsync((uint)FPedHash.Paramedic01SMM, this._spawnLocation, 0f);
        this._ambulance = await this._entityService.SpawnVehicleAsync((uint)Cfx.API.GetHashKey(serviceVehicle.Model), this._spawnLocation, 0f);
        
        this._ambulance.Properties.CreateBlip(FBlipSprite.Health, FBlipColor.Red, "Ambulance");
        this._ambulance.Properties.IsSirenActive(true);

        this._driver.Properties.SetAlwaysKeepTask(true);
        this._driver.Properties.SetIntoVehicle(this._ambulance.GetUnderlyingVehicle, FVehicleSeat.Driver);
        this._driver.Properties.SetBlockPermanentEvents(true);
        this._driver.Task.DriveTo(this._ambulance.GetUnderlyingVehicle, this._target, 0f, 30f, (int)FDrivingStyle.Emergency);
        
        while (this._ambulance.GetUnderlyingVehicle.Position.DistanceToSquared2D(this._target) >= 150f)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                break;
            }

            await Task.Delay(100);
        }
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
        this._ambulance.Properties.IsSirenActive(false);
        this._driver.Task.CruiseWithVehicle(this._ambulance.GetUnderlyingVehicle, 12f, (int)FDrivingStyle.Normal);
        Cfx.API.SetEntityDistanceCullingRadius(this._driver.GetUnderlyingPed.NetworkId, 100f);
        Cfx.API.SetEntityDistanceCullingRadius(this._ambulance.GetUnderlyingVehicle.NetworkId, 100f);
        this._driver.Properties.MarkAsNoLongerNeeded();
        this._ambulance.Properties.MarkAsNoLongerNeeded();
    }
}