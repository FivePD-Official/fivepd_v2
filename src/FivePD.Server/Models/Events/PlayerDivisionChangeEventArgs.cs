using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Models.Events;

/// <summary>
/// The event arguments for when a player joined or left a unit.
/// </summary>
public sealed record PlayerDivisionChangeEventArgs
{
    /// <summary>
    /// Gets the player.
    /// </summary>
    public FPlayer Player { get; init; }

    /// <summary>
    /// Gets the new division.
    /// </summary>
    public Division Division { get; init; }
}