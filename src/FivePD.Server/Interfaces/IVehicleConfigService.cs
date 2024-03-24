using System.Collections.Generic;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides access to vehicle config data.
/// </summary>
public interface IVehicleConfigService
{
    /// <summary>
    /// Gets all vehicle configs of a given type.
    /// </summary>
    /// <param name="type">The type of vehicles to retrieve.</param>
    /// <returns>A list of vehicle configs.</returns>
    public List<VehicleConfig<Dictionary<string, object>>> GetAll(VehicleConfigType type);

    /// <summary>
    /// Gets all vehicles configs of a given type and maps their Options field to the given <see cref="TOptions"/> type.
    /// </summary>
    /// <param name="type">The type of vehicles to retrieve.</param>
    /// <typeparam name="TOptions">The type to map the Options field.</typeparam>
    /// <returns>A list of vehicle configs.</returns>
    public List<VehicleConfig<TOptions>> GetAll<TOptions>(VehicleConfigType type);
}