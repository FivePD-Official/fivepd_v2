using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.API;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Models.DTO;

namespace FivePD.Server.Models;

/// <summary>
/// Represents a unit.
/// </summary>
public class Unit : IInternalUnit
{
    private readonly List<FPlayer> _players = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Unit"/> class.
    /// </summary>
    /// <param name="callsign">The callsign.</param>
    public Unit(string callsign)
    {
        this.Callsign = callsign;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Unit"/> class.
    /// </summary>
    /// <param name="callsign">The callsign.</param>
    /// <param name="player">A player to add by default.</param>
    public Unit(string callsign, FPlayer player)
    {
        this.Callsign = callsign;
        this._players.Add(player);
        this.Availability.Subscribe(this.ListenForAvailabilityChanges);
        this.Status.Subscribe(this.ListenForStatusChanges);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Unit"/> class.
    /// </summary>
    /// <param name="callsign">The callsign.</param>
    /// <param name="players">The players to add by default.</param>
    public Unit(string callsign, IEnumerable<FPlayer> players)
    {
        this.Callsign = callsign;
        this._players.AddRange(players);
        this.Availability.Subscribe(this.ListenForAvailabilityChanges);
        this.Status.Subscribe(this.ListenForStatusChanges);
    }

    /// <summary>
    /// Gets the player's call availability observable.
    /// </summary>
    public FObservable<FUnitAvailability> Availability { get; private set; } = new(FUnitAvailability.NotReceiving);

    /// <summary>
    /// Gets the player's status observable.
    /// </summary>
    public FObservable<string> Status { get; private set; } = new(string.Empty);

    /// <summary>
    /// Gets or sets the callsign.
    /// </summary>
    public string Callsign { get; set; }

    /// <summary>
    /// Gets the currently attached callout.
    /// </summary>
    public SimpleCallout Callout { get; internal set; }

    /// <summary>
    /// Gets the players in the unit.
    /// </summary>
    public ReadOnlyCollection<FPlayer> Players => this._players.AsReadOnly();

    /// <inheritdoc/>
    Callout IInternalUnit.InternalCallout { get; set; }

    /// <summary>
    /// Adds a player to the unit.
    /// </summary>
    /// <param name="player">The player to add.</param>
    void IInternalUnit.AddPlayer(FPlayer player)
    {
        if (this._players.FirstOrDefault(x => x.NetId == player.NetId) is null)
        {
            this._players.Add(player);
            player.UnitCallsign.Value = this.Callsign;
        }
    }

    /// <summary>
    /// Removes the player from the unit.
    /// </summary>
    /// <param name="player">The player to remove.</param>
    void IInternalUnit.RemovePlayer(FPlayer player)
    {
        this._players.RemoveAll(x => x.NetId == player.NetId);
        player.UnitCallsign.Value = string.Empty;
    }

    /// <summary>
    /// A.
    /// </summary>
    /// <returns>B.</returns>
    internal DispatcherUnitDTO ToDispatcherDTO()
    {
        return new DispatcherUnitDTO
        {
            Callsign = this.Callsign,
            Players = this.Players.Select(player => new SimplePlayerDTO(player.NetId, player.GetUnderlyingPlayer().Name)),
            Availability = this.Availability.Value,
            Status = this.Status.Value,
            AssignedCallout = this.Callout is null ? null : new SimpleCallout(((IInternalUnit)this).InternalCallout),
        };
    }

    /// <summary>
    /// A.
    /// </summary>
    /// <returns>B.</returns>
    internal CommonDispatcherUnitDTO ToCommonDispatcherDTO()
    {
        return new CommonDispatcherUnitDTO
        {
            Callsign = this.Callsign,
            Players = this.Players.Select(player => new SimplePlayerDTO(player.NetId, player.GetUnderlyingPlayer().Name)),
            Availability = this.Availability.Value,
            Status = this.Status.Value,
        };
    }

    private void ListenForAvailabilityChanges(FUnitAvailability availability)
    {
        foreach (var player in this._players)
        {
            player.GetUnderlyingPlayer().TriggerEvent(Common.Events.Player.UpdateUnitAvailability, availability);
        }
    }

    private void ListenForStatusChanges(string status)
    {
        foreach (var player in this._players)
        {
            player.GetUnderlyingPlayer().TriggerEvent(Common.Events.Player.UpdateUnitStatus, status);
        }
    }
}

#pragma warning disable

internal interface IInternalUnit
{
    public void AddPlayer(FPlayer player);

    public void RemovePlayer(FPlayer player);

    public Callout InternalCallout { get; set; }
}