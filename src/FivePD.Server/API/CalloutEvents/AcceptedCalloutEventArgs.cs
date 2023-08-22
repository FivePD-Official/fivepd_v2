using FivePD.Server.Models;

namespace FivePD.Server.API.CalloutEvents;

/// <summary>
/// The event arguments for when a callout is accepted.
/// </summary>
/// <param name="Callout">The callout.</param>
/// <param name="Unit">The unit.</param>
public sealed record AcceptedCalloutEventArgs(Callout Callout, Unit Unit);