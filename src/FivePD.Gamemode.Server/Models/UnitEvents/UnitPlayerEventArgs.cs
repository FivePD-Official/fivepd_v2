using FivePD.Gamemode.Server.EntityWrappers;

namespace FivePD.Gamemode.Server.Models.UnitEvents;

/// <summary>
/// The event arguments for when a player joined or left a unit.
/// </summary>
public sealed record UnitPlayerEventArgs
{
    /// <summary>
    /// Gets the unit.
    /// </summary>
    public Unit Unit { get; init; }

    /// <summary>
    /// Gets the player.
    /// </summary>
    public FPlayer Player { get; init; }
}