using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Server.EntityWrappers.PedWrapper;
using FivePD.Gamemode.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Models.AssistanceServiceModels;

#pragma warning disable

public class Ambulance : AssistanceService
{
    private readonly IEntityService _entityService;

    public Ambulance(IEntityService entityService)
    {
        this._entityService = entityService;
    }

    public override async Task SpawnEntitiesAsync()
    {
        Debug.WriteLine("SpawnEntities() called");

        var ped = await this._entityService.SpawnPedAsync(3008586398, this.Location, 0f);
        var vehicle = await this._entityService.SpawnVehicleAsync((uint)Cfx.API.GetHashKey("ambulance"), this.Location, 0f);

        var fped = FPed.FromPed(ped);
        fped.Properties.AlwaysKeepTask(true);
        fped.Properties.SetIntoVehicle(vehicle, FVehicleSeat.Driver);
        fped.Properties.BlockPermanentEvents(true);
    }

    public override async Task GoToLocationAsync()
    {
        Debug.WriteLine("GoToLocation() called");
    }

    public override Task PerformActionAsync()
    {
        Debug.WriteLine("PerformAction() called");
        return Task.FromResult(0);
    }

    public override Task ActionCompletedOrCanceledAsync()
    {
        Debug.WriteLine("ActionCompletedOrCanceled() called");
        return Task.FromResult(0);
    }
}