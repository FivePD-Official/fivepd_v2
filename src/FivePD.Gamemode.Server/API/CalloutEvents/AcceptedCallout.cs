using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.API.CalloutEvents;

/// <summary>
/// The event arguments for when a callout is accepted.
/// </summary>
public sealed record AcceptedCallout
{
    /// <summary>
    /// Gets the callout.
    /// </summary>
    public Callout Callout { get; init; }

    /// <summary>
    /// Gets the unit.
    /// </summary>
    public Unit Unit { get; init; }
}