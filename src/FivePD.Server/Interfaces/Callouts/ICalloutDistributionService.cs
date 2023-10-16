using System;
using FivePD.Server.API;
using FivePD.Server.API.CalloutEvents;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces.Callouts;

/// <summary>
/// Responsible for creating and distributing callouts.
/// </summary>
public interface ICalloutDistributionService
{
    /// <summary>
    /// Invoked when a callout is dispatched to units.
    /// </summary>
    public event EventHandler<DispatchedCalloutEventArgs> OnCalloutDispatched;

    /// <summary>
    /// Invoked when a callout is accepted.
    /// </summary>
    public event EventHandler<AcceptedCalloutEventArgs> OnCalloutAccepted;

    /// <summary>
    /// Invoked when a unit is assigned to a callout.
    /// </summary>
    public event EventHandler<UnitAssignedToCalloutEventArgs> OnUnitAssignedToCallout;

    /// <summary>
    /// Assigns a unit to a callout.
    /// </summary>
    /// <param name="unit">The unit.</param>
    /// <param name="callout">The callout.</param>
    /// <param name="acceptedByUnit">true = accepted by a unit; false = assigned by a dispatcher or an addon.</param>
    /// <returns>true if the unit was assigned to the callout; otherwise, false.</returns>
    public bool AssignUnitToCallout(Unit unit, Callout callout, bool acceptedByUnit = false);
}

/// <summary>
/// Responsible for distributing callouts to units or dispatchers.
/// </summary>
internal interface IInternalCalloutDistributionService
{
    /// <summary>
    /// Distributes a callout to units or dispatchers, according to the configuration.
    /// </summary>
    /// <param name="internalCallout">The callout.</param>
    /// <param name="manualTrigger">true = callout was triggered manually; false = callout was triggered automatically.</param>
    public void DistributeCallout(InternalCallout internalCallout, bool manualTrigger);
}