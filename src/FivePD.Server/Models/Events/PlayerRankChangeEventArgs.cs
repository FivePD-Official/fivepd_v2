using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Models.Events;

#pragma warning disable

/// <summary>
/// The event arguments for when a player joined or left a unit.
/// </summary>
public sealed record PlayerRankChangeEventArgs
{
    /// <summary>
    /// Gets the player.
    /// </summary>
    public FPlayer Player { get; init; }

    /// <summary>
    /// Gets the new rank.
    /// </summary>
    public Rank Rank { get; init; }
}