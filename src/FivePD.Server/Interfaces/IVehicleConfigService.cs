using System.Collections.Generic;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides access to vehicle config data.
/// </summary>
public interface IVehicleConfigService
{
    /// <summary>
    /// Retrieves all vehicle configurations of a specific type.
    /// </summary>
    /// <param name="type">The type of vehicle configuration to retrieve.</param>
    /// <returns>A list of vehicle configurations.</returns>
    public List<VehicleConfig<Dictionary<string, object>>> GetAll(VehicleConfigType type);

    /// <summary>
    /// Retrieves all vehicle configurations of a specific type, with options deserialized to a specified type.
    /// </summary>
    /// <param name="type">The type of vehicle configuration to retrieve.</param>
    /// <typeparam name="TOptions">The type to deserialize the options to.</typeparam>
    /// <returns>A list of vehicle configurations with deserialized options.</returns>
    public List<VehicleConfig<TOptions>> GetAll<TOptions>(VehicleConfigType type);
}
