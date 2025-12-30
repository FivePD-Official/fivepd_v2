using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Repositories;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class VehicleStorageService : IVehicleStorageService, IInternalVehicleStorageService
{
    private readonly ConcurrentDictionary<int, FVehicle> _vehicles = new();

    private readonly VehicleDataRepository _vehicleDataRepository;

    public ReadOnlyDictionary<int, FVehicle> Vehicles => new(this._vehicles);

    ConcurrentDictionary<int, FVehicle> IInternalVehicleStorageService.InternalVehicles => this._vehicles;

    public VehicleStorageService(VehicleDataRepository vehicleDataRepository)
    {
        this._vehicleDataRepository = vehicleDataRepository;
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

        return this._vehicleDataRepository.FindByIdWithDynamicRelated(
            vehicle.VehicleDataId,
            new VehicleDataRepository.VehicleDataLoadOptions { IncludeDocuments = true });
    }
}