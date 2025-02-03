using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

#pragma warning disable

public class VehicleStorageService : IVehicleStorageService, IInternalVehicleStorageService
{
    private readonly ConcurrentDictionary<int, FVehicle> _vehicles = new();
    
    public ReadOnlyDictionary<int, FVehicle> Vehicles => new(this._vehicles);

    ConcurrentDictionary<int, FVehicle> IInternalVehicleStorageService.InternalVehicles => this._vehicles;

    public FVehicle TryGetVehicle(int networkId)
    {
        var found = this._vehicles.TryGetValue(networkId, out var vehicle);
        if (found)
        {
            return vehicle;
        }

        vehicle = FVehicle.FromNetId(networkId); // TODO: this should not exist, it's an empty instance from the builder (??) same in PedService
        if (vehicle is null)
        {
            return null;
        }

        this._vehicles[networkId] = vehicle;

        return vehicle;
    }
}