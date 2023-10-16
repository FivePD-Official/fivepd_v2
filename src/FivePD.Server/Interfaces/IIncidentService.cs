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
internal interface IInternalIncidentService : IIncidentService
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
    /// Assigns the specified unit to the specified incident.
    /// </summary>
    /// <param name="unit">The unit to assign.</param>
    /// <param name="incident">The incident to assign the unit to.</param>
    /// <returns><see langword="true"/> if the unit was assigned to the incident; otherwise, <see langword="false"/>.</returns>
    public bool AssignUnitToIncident(Unit unit, IIncident incident);
}