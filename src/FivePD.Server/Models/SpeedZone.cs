using CitizenFX.Core;

namespace FivePD.Server.Models;

/// <summary>
/// Represents a speed zone.
/// </summary>
public class SpeedZone
{
    /// <summary>
    /// Gets the position.
    /// </summary>
    public Vector3 Position { get; init; }

    /// <summary>
    /// Gets the radius.
    /// </summary>
    public int Radius { get; init; }

    /// <summary>
    /// Gets the speed.
    /// </summary>
    public int Speed { get; init; }
}