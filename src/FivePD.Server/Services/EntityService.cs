using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class EntityService : IEntityService
{
    private readonly IEventService _eventService;
    private readonly IPedService _pedService;
    private readonly IVehicleService _vehicleService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityService"/> class.
    /// </summary>
    /// <param name="eventService">The <see cref="IEventService"/> to use.</param>
    /// <param name="pedService">The <see cref="IPedService"/> to use.</param>
    /// <param name="vehicleService">The <see cref="IVehicleService"/> to use.</param>
    public EntityService(
        IEventService eventService,
        IPedService pedService,
        IVehicleService vehicleService)
    {
        this._eventService = eventService;
        this._pedService = pedService;
        this._vehicleService = vehicleService;
    }

    /// <inheritdoc />
    public async Task<FPed> SpawnPedAsync(uint model, Vector3 location, float heading)
    {
        var handle = Cfx.API.CreatePed(26, model, location.X, location.Y, location.Z, heading, true, false);

        while (!Cfx.API.DoesEntityExist(handle))
        {
            // TODO: maybe use Task instead of BaseScript.Delay?
            await BaseScript.Delay(100);
        }

        Cfx.API.SetEntityDistanceCullingRadius(handle, 10000f);

        return FPed.FromPed(new Ped(handle));
    }

    /// <inheritdoc />
    public async Task<FVehicle> SpawnVehicleAsync(uint model, Vector3 location, float heading)
    {
        // TODO: clear area
        var handle = Cfx.API.CreateVehicle(model, location.X, location.Y, location.Z, heading, true, false);

        while (!Cfx.API.DoesEntityExist(handle))
        {
            await BaseScript.Delay(100);
        }

        Cfx.API.SetEntityDistanceCullingRadius(handle, 10000f);

        return FVehicle.FromVehicle(new Vehicle(handle));
    }

    /// <inheritdoc />
    public async Task<Prop> SpawnPropAsync(int model, Vector3 location)
    {
        var handle = Cfx.API.CreateObject(model, location.X, location.Y, location.Z, true, false, false);

        while (!Cfx.API.DoesEntityExist(handle))
        {
            await BaseScript.Delay(100);
        }

        Cfx.API.SetEntityDistanceCullingRadius(handle, 10000f);

        return new Prop(handle);
    }

    /// <inheritdoc />
    public async Task<T> GetClosestEntityToPlayerAsync<T>(Player player)
        where T : FEntity
    {
        var type = string.Empty;
        if (typeof(T) == typeof(FPed))
        {
            type = "ped";
        }
        else if (typeof(T) == typeof(FVehicle))
        {
            type = "vehicle";
        }

        var networkId = await this._eventService.TriggerEventStrictAsync(player, Events.Entity.GetClosestEntityToPlayer, type);
        if (networkId == "-1")
        {
            return null;
        }

        if (typeof(T) == typeof(FPed))
        {
            var fPed = this._pedService.TryGetPed(int.Parse(networkId));
            return fPed as T;
        }

        if (typeof(T) == typeof(FVehicle))
        {
            var fVehicle = this._vehicleService.TryGetVehicle(int.Parse(networkId));
            return fVehicle as T;
        }

        return null;
    }
}