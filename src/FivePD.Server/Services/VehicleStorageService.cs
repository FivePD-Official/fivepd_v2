using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class VehicleStorageService : IVehicleStorageService, IInternalVehicleStorageService
{
    private readonly ConcurrentDictionary<int, FVehicle> _vehicles = new();
    
    private readonly IDatabaseContextFactory _dbContextFactory;
    
    public ReadOnlyDictionary<int, FVehicle> Vehicles => new(this._vehicles);

    ConcurrentDictionary<int, FVehicle> IInternalVehicleStorageService.InternalVehicles => this._vehicles;

    public VehicleStorageService(IDatabaseContextFactory dbContextFactory)
    {
        this._dbContextFactory = dbContextFactory;
    }

    public FVehicle TryGetVehicle(int networkId)
    {
        var found = this._vehicles.TryGetValue(networkId, out var vehicle);
        if (found)
        {
            return vehicle;
        }

        vehicle = FVehicle.FromNetId(networkId);
        this._vehicles[networkId] = vehicle;

        return vehicle;
    }
    
    public VehicleData GetVehicleData(FVehicle vehicle)
    {
        if (vehicle is null)
        {
            return null;
        }
        
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        return databaseContext.VehicleData.FirstOrDefault(x => x.Id == vehicle.VehicleDataId);
    }
}