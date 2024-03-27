using System.Collections.Generic;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides access to location-related data.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Gets all locations of the given type.
    /// </summary>
    /// <param name="type">The type to filter to.</param>
    /// <returns>A list of <see cref="Location"/>.</returns>
    public List<Location> GetAll(LocationType type);
}