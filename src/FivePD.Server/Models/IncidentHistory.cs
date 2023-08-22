using System;
using System.Collections.Generic;
using FivePD.Common.Enums;

namespace FivePD.Server.Models;

/// <summary>
/// Represents an incident (callout or traffic stop) history.
/// </summary>
public class IncidentHistory
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets the title of the incident.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the description of the incident.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the location of the incident.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Gets or sets the priority of the incident.
    /// </summary>
    public FIncidentPriority Priority { get; set; }

    /// <summary>
    /// Gets or sets the type of the incident.
    /// </summary>
    public FIncidentType Type { get; set; }

    /// <summary>
    /// Gets the generated date time of the incident.
    /// </summary>
    public DateTime GeneratedAt { get; }

    /// <summary>
    /// Gets or sets the users who were involved in this incident.
    /// </summary>
    public IEnumerable<IncidentHistoryUser> Users { get; set; }
}