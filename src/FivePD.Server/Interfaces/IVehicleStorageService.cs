using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// A service for managing and accessing vehicle data.
/// </summary>
public interface IVehicleStorageService
{
    /// <summary>
    /// Gets a readonly dictionary of all vehicles on the server.
    /// </summary>
    public ReadOnlyDictionary<int, FVehicle> Vehicles { get; }

    /// <summary>
    /// Tries to get a <see cref="FVehicle"/> by its network ID.
    /// </summary>
    /// <param name="networkId">The vehicle's network ID.</param>
    /// <returns>The vehicle or null.</returns>
    public FVehicle TryGetVehicle(int networkId);

    /// <summary>
    /// Gets the <see cref="VehicleData"/> of the given <see cref="FVehicle"/>.
    /// </summary>
    /// <param name="vehicle">The vehicle.</param>
    /// <returns>The VehicleHistory of the vehicle.</returns>
    public VehicleData GetVehicleData(FVehicle vehicle);
}

internal interface IInternalVehicleStorageService
{
    public ConcurrentDictionary<int, FVehicle> InternalVehicles { get; }
}