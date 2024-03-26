using System.Collections.Generic;
using FivePD.Client.Models;

namespace FivePD.Client.Interfaces;

/// <summary>
/// Provides location-related data.
/// </summary>
internal interface ILocationService
{
    /// <summary>
    /// Gets the locations.
    /// </summary>
    public List<Location> Locations { get; }
}