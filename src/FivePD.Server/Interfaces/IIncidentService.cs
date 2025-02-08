using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FivePD.Server.API;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Represents the service that handles incidents.
/// </summary>
public interface IIncidentService
{
    /// <summary>
    /// Invoked when a callout is completed.
    /// </summary>
    public event EventHandler<Callout> OnCalloutCompleted;

    /// <summary>
    /// Gets the active callouts.
    /// </summary>
    public ReadOnlyCollection<Callout> ActiveCallouts { get; }

    /// <summary>
    /// Gets the active traffic stops.
    /// </summary>
    public ReadOnlyCollection<TrafficStop> TrafficStops { get; }

    /// <summary>
    /// Gets the incident of the specified player.
    /// </summary>
    /// <param name="player">The player to get the incident of.</param>
    /// <returns>The incident of the specified player.</returns>
    public IIncident GetCurrentIncidentOfPlayer(FPlayer player);

    /// <summary>
    /// Retrieves the incident with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the incident to retrieve.</param>
    /// <returns>The incident associated with the specified identifier.</returns>
    public IIncident GetById(Guid id);

    /// <summary>
    /// Completes the incident of the specified player.
    /// </summary>
    /// <param name="player">The player to complete the incident of.</param>
    public void CompleteIncidentOfPlayer(FPlayer player);

    /// <summary>
    /// Completes the specified incident.
    /// </summary>
    /// <param name="incident">The incident to complete.</param>
    public void CompleteIncident(IIncident incident);
}

/// <summary>
/// Represents the service that handles incidents.
/// </summary>
internal interface IInternalIncidentService
{
    /// <summary>
    /// Gets the internal callouts.
    /// </summary>
    public List<InternalCallout> InternalCallouts { get; }

    /// <summary>
    /// Gets the internal traffic stops.
    /// </summary>
    public List<TrafficStop> InternalTrafficStops { get; }

    /// <summary>
    /// Assigns the specified unit to the given incident.
    /// </summary>
    /// <param name="unit">The unit to assign to the incident.</param>
    /// <param name="incident">The incident to which the unit should be assigned.</param>
    /// <returns>A boolean value indicating whether the unit was successfully assigned to the incident.</returns>
    public bool AssignUnitToIncident(Unit unit, IIncident incident); // TODO: this can be public I think

    /// <summary>
    /// Unassigns the specified unit from the given incident.
    /// </summary>
    /// <param name="unit">The unit to be unassigned from the incident.</param>
    /// <param name="incident">The incident from which the unit will be unassigned.</param>
    /// <returns>A boolean value indicating whether the unit was successfully unassigned from the incident.</returns>
    public bool UnassignUnitFromIncident(Unit unit, IIncident incident); // TODO: this can be public I think
}