using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Models;
using Newtonsoft.Json;

namespace FivePD.Server.API;

#pragma warning disable VSTHRD200

/// <summary>
/// Represents a callout.
/// </summary>
public abstract class Callout : IIncident, IInternalIncident
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
    public FPriority Priority { get; internal set; }

    /// <summary>
    /// Gets the activity radius of the callout.
    /// When a player enters in this radius, the <see cref="Callout.OnPlayerEnteredInActivityZone"/> will be invoked.
    /// </summary>
    public float ActivityRadius { get; internal set; }

    /// <inheritdoc />
    public Vector3 Location { get; internal set; }

    /// <inheritdoc />
    public ReadOnlyCollection<Unit> Units => this._units.AsReadOnly();

    /// <inheritdoc />
    public DateTime GeneratedAt { get; internal set; }

    /// <summary>
    /// Invoked before <see cref="CanBeDispatched"/>.
    /// </summary>
    /// <returns>The properties of the callout.</returns>
    public abstract Task<CalloutSetupProperties> Setup();

    /// <summary>
    /// Invoked before sent to the players or the dispatchers.
    /// </summary>
    /// <returns>A bool whether the callout should be dispatched or not.</returns>
    public abstract Task<bool> CanBeDispatched();

    /// <summary>
    /// Invoked only when DispatchType is "auto" and before <see cref="OnAccepted"/>.
    /// </summary>
    /// <param name="units">The units that were notified about this callout and can accept to respond.</param>
    /// <returns>A Task.</returns>
    public abstract Task OnDispatched(List<Unit> units);

    /// <summary>
    /// Invoked when a dispatcher or an addon assigns a unit to the callout.
    /// </summary>
    /// <param name="unit">The unit that has been assigned to the callout.</param>
    /// <returns>A Task.</returns>
    public abstract Task OnUnitAssigned(Unit unit);

    /// <summary>
    /// Invoked only when a unit accepts to respond to the callout.
    /// </summary>
    /// <param name="unit">The unit that accepted to respond.</param>
    /// <returns>A Task.</returns>
    public abstract Task OnAccepted(Unit unit);

    /// <summary>
    /// Invoked when a player enters in the activity zone of the callout.
    /// </summary>
    /// <param name="player">The player who entered.</param>
    /// <returns>A Task.</returns>
    public abstract Task OnPlayerEnteredInActivityZone(FPlayer player);

    /// <summary>
    /// Invoked when any assigned unit completes the callout.
    /// </summary>
    /// <returns>A Task.</returns>
    public abstract Task OnCompleted(); // TODO: Should be invoked when all players are Code 4 or a player has pressed the "End incident" button

#pragma warning disable
    internal DispatcherCalloutDTO ToCalloutForDispatcherDTO()
    {
        return new DispatcherCalloutDTO
        {
            Id = this.Id.ToString(),
            Title = this.Title,
            Description = this.Description,
            Priority = this.Priority,
            Location = this.Location,
            GeneratedAt = this.GeneratedAt,
        };
    }

    /// <summary>
    /// Gets or sets the units.
    /// </summary>
    List<Unit> IInternalIncident.Units
    {
        get => this._units;
        set => this._units = value;
    }
}