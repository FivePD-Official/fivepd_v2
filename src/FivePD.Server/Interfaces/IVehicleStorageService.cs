using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IVehicleStorageService
{
    public ReadOnlyDictionary<int, FVehicle> Vehicles { get; }

    /// <summary>
    /// Tries to get a <see cref="FVehicle"/> by its network ID.
    /// </summary>
    /// <param name="networkId">The vehicle's network ID.</param>
    /// <returns>The vehicle or null.</returns>
    public FVehicle TryGetVehicle(int networkId);
}

internal interface IInternalVehicleStorageService
{
    public ConcurrentDictionary<int, FVehicle> InternalVehicles { get; }
}