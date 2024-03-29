using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FivePD.Common.Enums;
using FivePD.Server.Utilities;

namespace FivePD.Server.Models;

/// <summary>
/// Represents an incident (callout or traffic stop) history.
/// </summary>
[Table("fivepd_incident_histories")]
public class IncidentHistory
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public string Id { get; set; } = IdGenerator.Generate();

    /// <summary>
    /// Gets or sets the identifier which is/was used to runtime reference this record.
    /// </summary>
    public string IncidentId { get; set; }

    /// <summary>
    /// Gets or sets the title of the incident.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the incident.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the location of the incident.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// Gets or sets the priority of the incident.
    /// </summary>
    public FPriority Priority { get; set; }

    /// <summary>
    /// Gets or sets the type of the incident.
    /// </summary>
    public FIncidentType Type { get; set; }

    /// <summary>
    /// Gets or sets the generated date time of the incident.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Gets or sets the users who were involved in this incident.
    /// </summary>
    public IList<IncidentHistoryUser> Users { get; set; }
}