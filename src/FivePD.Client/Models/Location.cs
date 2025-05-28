using CitizenFX.Core;

namespace FivePD.Client.Models;

/// <summary>
/// Represents a location.
/// </summary>
internal class Location
{
    /// <summary>
    /// Gets or sets the identifier of the location.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the location.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the location.
    /// </summary>
    public LocationType Type { get; set; }

    /// <summary>
    /// Gets or sets the position of the location.
    /// </summary>
    public Vector3 Position { get; set; }
}