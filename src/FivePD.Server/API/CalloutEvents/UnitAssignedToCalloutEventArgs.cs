using FivePD.Server.Models;

namespace FivePD.Server.API.CalloutEvents;

/// <summary>
/// The event arguments for when a unit is assigned to a callout.
/// </summary>
/// <param name="Callout">The callout.</param>
/// <param name="Unit">The unit.</param>
public sealed record UnitAssignedToCalloutEventArgs(Callout Callout, Unit Unit);