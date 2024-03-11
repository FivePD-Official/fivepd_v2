using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Models.Events;

#pragma warning disable

/// <summary>
/// The event arguments for when the duty status of a player is changed.
/// </summary>
public sealed record PlayerDutyStatusChangeEventArgs
{
    /// <summary>
    /// Gets the player.
    /// </summary>
    public FPlayer Player { get; init; }

    /// <summary>
    /// Gets the new status.
    /// </summary>
    public FDutyStatus Status { get; init; }
}