using System;
using FivePD.Gamemode.Server.API.CalloutEvents;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Responsible for creating and distributing callouts.
/// </summary>
internal interface ICalloutDistributionService
{
    /// <summary>
    /// Invoked when a callout is dispatched.
    /// </summary>
    public event EventHandler<DispatchedCallout> OnCalloutDispatched;

    /// <summary>
    /// Invoked when a callout is accepted.
    /// </summary>
    public event EventHandler<AcceptedCallout> OnCalloutAccepted;
}