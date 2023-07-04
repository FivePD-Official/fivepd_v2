using FivePD.Gamemode.Server.EntityWrappers;

namespace FivePD.Gamemode.Server.Models.Events;

#pragma warning disable

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