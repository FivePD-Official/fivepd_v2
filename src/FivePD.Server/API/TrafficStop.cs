using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Models;

namespace FivePD.Server.API;

#pragma warning disable VSTHRD200

/// <summary>
/// Represents a traffic stop.
/// </summary>
public class TrafficStop : IIncident, IInternalIncident
{
    private List<Unit> _units = new();

    /// <inheritdoc />
    public Guid Id { get; internal set; }

    /// <inheritdoc />
    public string Title { get; internal set; }

    /// <inheritdoc />
    public string Description { get; internal set; }

    /// <summary>
    /// Gets the priority of the callout.
    /// </summary>
    public FIncidentPriority Priority { get; internal set; }

    /// <inheritdoc />
    public Vector3 Location { get; internal set; }

    /// <inheritdoc />
    public ReadOnlyCollection<Unit> Units => this._units.AsReadOnly();

    /// <inheritdoc />
    public DateTime GeneratedAt { get; internal set; }

    /// <summary>
    /// Gets the vehicle involved in the traffic stop.
    /// </summary>
    public FVehicle Vehicle { get; internal set; }

    /// <summary>
    /// Gets or sets the units.
    /// </summary>
#pragma warning disable S2292 // ???
    List<Unit> IInternalIncident.Units
#pragma warning restore S2292
    {
        get => this._units;
        set => this._units = value;
    }
}