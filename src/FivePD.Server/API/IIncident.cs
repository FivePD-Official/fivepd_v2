using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Server.Models;

namespace FivePD.Server.API;

/// <summary>
/// Represents an incident. Main usage is callouts and traffic stop.
/// </summary>
public interface IIncident
{
    /// <summary>
    /// Gets the unique identifier of the incident.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the title of the incident.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the description of the incident.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the priority of the incident.
    /// </summary>
    public FPriority Priority { get; }

    /// <summary>
    /// Gets the location of the incident.
    /// </summary>
    public Vector3 Location { get; }

    /// <summary>
    /// Gets the attached units to the incident.
    /// </summary>
    public ReadOnlyCollection<Unit> Units { get; }

    /// <summary>
    /// Gets the generated date time of the incident.
    /// </summary>
    public DateTime GeneratedAt { get; }
}

/// <summary>
/// Represents an incident with its internal methods.
/// </summary>
internal interface IInternalIncident
{
    /// <summary>
    /// Gets or sets the units.
    /// </summary>
    public List<Unit> Units { get; set; }
}