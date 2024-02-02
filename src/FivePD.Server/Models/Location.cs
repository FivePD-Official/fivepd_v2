using System;
using CitizenFX.Core;

namespace FivePD.Server.Models;

/// <summary>
/// Represents a location to be used for markers in UI.
/// </summary>
public class Location
{
    /// <summary>
    /// Gets or sets a location's identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a location's type.
    /// </summary>
    public LocationType Type { get; set; }

    /// <summary>
    /// Gets or sets a location's position.
    /// </summary>
    public Vector3 Position { get; set; }
}