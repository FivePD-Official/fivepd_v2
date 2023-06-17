using System.Collections.Generic;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.API.CalloutEvents;

/// <summary>
/// The event arguments for when a callout is dispatched.
/// </summary>
public sealed record DispatchedCallout
{
    /// <summary>
    /// Gets the callout.
    /// </summary>
    public Callout Callout { get; init; }

    /// <summary>
    /// Gets the notified units.
    /// </summary>
    public List<Unit> NotifiedUnits { get; init; }
}