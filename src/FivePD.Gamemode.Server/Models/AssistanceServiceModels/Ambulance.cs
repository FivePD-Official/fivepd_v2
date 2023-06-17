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

public class Ambulance : AssistanceService
{
    private readonly IEntityService _entityService;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;

    private FPed _driver;
    private FVehicle _ambulance;

    public Ambulance(IEntityService entityService, IConfigService configService, IRandomService randomService)
    {
        this._entityService = entityService;
        this._configService = configService;
        this._randomService = randomService;
    }

    public override async Task OnCalledAsync(CancellationToken cancellationToken)
    {
        Debug.WriteLine("SpawnEntities() called");
        var serviceVehicle = this._configService.VehiclesConfig.Ambulance[this._randomService.Random.Next(0, this._configService.VehiclesConfig.Ambulance.Count)];

        var ped = await this._entityService.SpawnPedAsync((uint)FPedHash.Paramedic01SMM, this.SpawnLocation, 0f);
        var vehicle = await this._entityService.SpawnVehicleAsync((uint)Cfx.API.GetHashKey(serviceVehicle.Model), this.SpawnLocation, 0f);
        
        this._ambulance = FVehicle.FromVehicle(vehicle);
        this._ambulance.Properties.CreateBlip(FBlipSprite.Health, FBlipColor.Red, "Ambulance");
        this._ambulance.Properties.IsSirenActive(true);

        this._driver = FPed.FromPed(ped);
        this._driver.Properties.AlwaysKeepTask(true);
        this._driver.Properties.SetIntoVehicle(vehicle, FVehicleSeat.Driver);
        this._driver.Properties.BlockPermanentEvents(true);
        this._driver.Task.DriveTo(this._ambulance.GetUnderlyingVehicle, this.Target, 0f, 30f, (int)FDrivingStyle.Emergency);
        
        while (this._ambulance.GetUnderlyingVehicle.Position.DistanceToSquared2D(this.Target) >= 150f)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await Task.Delay(100);
        }
    }

    public override Task OnCancellationRequestedAsync()
    {
        Debug.WriteLine("ActionCompletedOrCanceled() called");
        this._ambulance.Properties.IsSirenActive(false);

        return Task.FromResult(0);
    }

    public override Task OnCompleteAsync()
    {
        Debug.WriteLine("OnCompleteAsync() called");
        this._ambulance.Properties.IsSirenActive(false);

        return Task.FromResult(0);
    }
}