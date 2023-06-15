using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.API;

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods

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
    public FIncidentPriority Priority { get; internal set; }

    /// <summary>
    /// Gets the activity radius of the callout.
    /// When a player enters in this radius, the <see cref="Callout.OnPlayerEnteredInActivityZone"/> will be invoked.
    /// </summary>
    public float ActivityRadius { get; internal set; }

    /// <inheritdoc />
    public Vector3 Location { get; internal set; }

    /// <inheritdoc />
    public ReadOnlyCollection<Unit> Units => this._units.AsReadOnly();

    /// <summary>
    /// Invoked before <see cref="Callout.CanBeDispatched"/>.
    /// </summary>
    /// <returns>The properties of the callout.</returns>
    public abstract Task<CalloutSetupProperties> Setup();

    /// <summary>
    /// Invoke before <see cref="Callout.OnDispatched"/>.
    /// </summary>
    /// <returns>A bool whether the callout should be dispatched or not.</returns>
    public abstract Task<bool> CanBeDispatched();

    /// <summary>
    /// Invoked before <see cref="Callout.OnAccepted"/>.
    /// </summary>
    /// <param name="notifiedUnits">The units that were notified about this callout and can accept to respond.</param>
    /// <returns>A Task.</returns>
    public abstract Task OnDispatched(List<Unit> notifiedUnits);

    /// <summary>
    /// Invoke when a unit accepts the callout.
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
    /// Invoked when any attached player completes the callout.
    /// </summary>
    /// <returns>A Task.</returns>
    public abstract Task OnCompleted(); // TODO: Should be invoked when all players are Code 4 or a player has pressed the "End incident" button

    /// <summary>
    /// Gets or sets the units.
    /// </summary>
#pragma warning disable
    List<Unit> IInternalIncident.Units
    {
        get => this._units;
        set => this._units = value;
    }
#pragma warning restore
}