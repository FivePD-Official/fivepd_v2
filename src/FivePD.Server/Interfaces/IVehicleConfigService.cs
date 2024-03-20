using System.Collections.Generic;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides access to vehicle config data.
/// </summary>
public interface IVehicleConfigService
{
    /// <summary>
    /// A.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A list of vehicle configs.</returns>
    public List<VehicleConfig<Dictionary<string, object>>> GetAll(VehicleConfigType type);

    /// <summary>
    /// A.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <typeparam name="TOptions">The type to map the Options field.</typeparam>
    /// <returns>A list of vehicle configs.</returns>
    public List<VehicleConfig<TOptions>> GetAll<TOptions>(VehicleConfigType type);
}