using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
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
    public async Task<FPed> SpawnPedAsync(uint model, Vector3 location, float heading, bool clearArea = false)
    {
        if (clearArea)
        {
            this.ClearArea(location);
        }

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
    public async Task<FVehicle> SpawnVehicleAsync(uint model, Vector3 location, float heading, bool clearArea = false)
    {
        if (clearArea)
        {
            this.ClearArea(location);
        }

        var handle = Cfx.API.CreateVehicle(model, location.X, location.Y, location.Z, heading, true, false);

        while (!Cfx.API.DoesEntityExist(handle))
        {
            await BaseScript.Delay(100);
        }

        Cfx.API.SetEntityDistanceCullingRadius(handle, 10000f);
        return FVehicle.FromVehicle(new Vehicle(handle));
    }

    /// <inheritdoc />
    public async Task<Prop> SpawnPropAsync(int model, Vector3 location, bool clearArea = false)
    {
        if (clearArea)
        {
            this.ClearArea(location);
        }

        var handle = Cfx.API.CreateObject(model, location.X, location.Y, location.Z, true, false, false);

        while (!Cfx.API.DoesEntityExist(handle))
        {
            await BaseScript.Delay(100);
        }

        Cfx.API.SetEntityDistanceCullingRadius(handle, 10000f);

        return new Prop(handle);
    }

    /// <inheritdoc />
    public List<Ped> GetAllPeds()
    {
        if (Cfx.API.GetAllPeds() is not List<object> pedIds)
        {
            return new List<Ped>();
        }

        foreach (var id in pedIds)
        {
            Debug.WriteLine(
                $"{Convert.ToInt32(id)} - {Cfx.API.DoesEntityExist(Convert.ToInt32(id))} - net id: {Cfx.API.NetworkGetNetworkIdFromEntity(Convert.ToInt32(id))}");
        }

        return pedIds
            .Select(Convert.ToInt32)
            .Select(x => new Ped(x))
            .ToList();
    }

    /// <inheritdoc />
    public List<FPed> GetAllFPeds()
    {
        return this.GetAllPeds()
            .Select(x => this._pedService.TryGetPed(x.NetworkId))
            .ToList();
    }

    /// <inheritdoc />
    public Ped GetClosestPedToEntity(Entity entity, float range = 15f)
    {
        return this.GetAllPeds()
            .OrderBy(x => x.Position.DistanceToSquared(entity.Position))
            .FirstOrDefault(x => x.NetworkId != entity.NetworkId && x.Position.DistanceToSquared(entity.Position) <= range);
    }

    /// <inheritdoc />
    public FPed GetClosestFPedToEntity(Entity entity, float range = 15f)
    {
        var ped = this.GetClosestPedToEntity(entity, range);
        return ped is null ? null : this._pedService.TryGetPed(ped.NetworkId);
    }

    /// <inheritdoc />
    public List<Ped> GetPedsInRangeOfPosition(Vector3 pos, float range = 15f)
    {
        return this.GetAllPeds()
            .Where(x => x.Position.DistanceToSquared(pos) <= range).ToList();
    }

    /// <inheritdoc />
    public List<Ped> GetPedsInRangeOfEntity(Entity entity, float range = 15f)
    {
        return this.GetPedsInRangeOfPosition(entity.Position, range);
    }

    /// <inheritdoc />
    public List<Vehicle> GetAllVehicles()
    {
        if (Cfx.API.GetAllVehicles() is not List<object> vehicleIds)
        {
            return new List<Vehicle>();
        }

        return vehicleIds
            .Select(Convert.ToInt32)
            .Select(x => new Vehicle(x))
            .ToList();
    }

    /// <inheritdoc />
    public List<FVehicle> GetAllFVehicles()
    {
        return this.GetAllVehicles()
            .Select(x => this._vehicleService.TryGetVehicle(x.NetworkId))
            .ToList();
    }

    /// <inheritdoc />
    public Vehicle GetClosestVehicleToEntity(Entity entity, float range = 15f)
    {
        return this.GetAllVehicles()
            .OrderBy(x => x.Position.DistanceToSquared(entity.Position))
            .FirstOrDefault(x => x.NetworkId != entity.NetworkId && Math.Sqrt(x.Position.DistanceToSquared(entity.Position)) <= range);
    }

    /// <inheritdoc />
    public FVehicle GetClosestFVehicleToEntity(Entity entity, float range = 15f)
    {
        var vehicle = this.GetClosestVehicleToEntity(entity, range);
        return vehicle is null ? null : this._vehicleService.TryGetVehicle(vehicle.NetworkId);
    }

    /// <inheritdoc />
    public List<Vehicle> GetVehiclesInRangeOfPosition(Vector3 pos, float range = 15f)
    {
        return this.GetAllVehicles()
            .Where(x => Math.Sqrt(x.Position.DistanceToSquared(pos)) <= range).ToList();
    }

    /// <inheritdoc />
    public List<Vehicle> GetVehiclesInRangeOfEntity(Entity entity, float range = 15f)
    {
        return this.GetVehiclesInRangeOfPosition(entity.Position, range);
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

    /// <inheritdoc />
    public void ClearArea(Vector3 pos, float radius = 5f)
    {
        foreach (var ped in this.GetPedsInRangeOfPosition(pos, radius).Where(ped => !Cfx.API.IsPedAPlayer(ped.Handle)))
        {
            Cfx.API.DeleteEntity(ped.Handle);
        }

        foreach (var vehicle in this.GetVehiclesInRangeOfPosition(pos, radius))
        {
            Cfx.API.DeleteEntity(vehicle.Handle);
        }
    }
}