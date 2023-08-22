using System.Collections.Generic;
using FivePD.Server.Models;

namespace FivePD.Server.API.CalloutEvents;

/// <summary>
/// The event arguments for when a callout is dispatched.
/// </summary>
/// <param name="Callout">The callout.</param>
/// <param name="Units">The units.</param>
public sealed record DispatchedCalloutEventArgs(Callout Callout, List<Unit> Units);