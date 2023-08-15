using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Server.API;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.IA.AmbientEvents;

#pragma warning disable

public class AmbientEvents : IAddon
{
    private readonly IAddonLoggerService _logger;
    private readonly IEntityService _entityService;
    private readonly ICfxService _cfxService;
    private readonly IUnitService _unitService;

    public AmbientEvents(IAddonLoggerService logger, IEntityService entityService, ICfxService cfxService, IUnitService unitService)
    {
        this._logger = logger;
        this._entityService = entityService;
        this._cfxService = cfxService;
        this._unitService = unitService;
    }

    public async Task OnStartedAsync()
    {
        this._logger.Information("I started!");

        // var ped = await this._entityService.SpawnPedAsync((uint)FPedHash.Agent, new Vector3(1619.16919f, 1192.42688f, 84.6966248f), 0f);
        // var vehicle = await this._entityService.SpawnVehicleAsync((uint)Cfx.API.GetHashKey("police"), new Vector3(1619.16919f, 1192.42688f, 84.6966248f), 0f);
        // this._cfxService.SetPedIntoVehicle(ped.GetUnderlyingPed.NetworkId, vehicle.GetUnderlyingVehicle.NetworkId, (int)FVehicleSeat.Driver);
        // ped.Task.CruiseWithVehicle(vehicle.GetUnderlyingVehicle, 30f, (int)FDrivingStyle.Emergency);
        // this._unitService.AddPlayerToUnit(new FPlayer(ped.GetUnderlyingPed.NetworkId.ToString()), this._unitService.Units().ElementAt(0));
    }

    public Task OnStoppedAsync()
    {
        this._logger.Information("I stopped!");

        return Task.CompletedTask;
    }
}