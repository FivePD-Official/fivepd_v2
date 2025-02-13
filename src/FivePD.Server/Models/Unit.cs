using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FivePD.Client.DTO;
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
public class Unit
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
    /// The DepartmentId will be set to the player's department id.
    /// </summary>
    /// <param name="callsign">The callsign.</param>
    /// <param name="player">A player to add by default.</param>
    public Unit(string callsign, FPlayer player)
    {
        this._players.Add(player);

        this.Callsign = callsign;
        this.DepartmentId = player.Department?.Id ?? 0;

        this.Availability.Subscribe(this.ListenForAvailabilityChanges);
        this.Status.Subscribe(this.ListenForStatusChanges);
        this.PanicMode.Subscribe(this.ListenForPanicModeChanges);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Unit"/> class.
    /// The DepartmentId will be set to the first player's department id.
    /// Players without a department will be ignored.
    /// Players with a different department will be ignored.
    /// </summary>
    /// <param name="callsign">The callsign.</param>
    /// <param name="players">The players to add by default.</param>
    public Unit(string callsign, IEnumerable<FPlayer> players)
    {
        var playersWithDepartment = players
            .Select(x => new Tuple<FPlayer, Department>(x, x.Department))
            .Where(x => x.Item2 is not null)
            .ToList();
        var firstPlayerDepartment = playersWithDepartment.First().Item2;
        var playersWithSameDepartment = playersWithDepartment.Where(x => x.Item2?.Id == firstPlayerDepartment?.Id).ToList();

        this._players.AddRange(playersWithSameDepartment.Select(x => x.Item1));

        this.Callsign = callsign;
        this.DepartmentId = playersWithSameDepartment.First().Item2?.Id ?? 0;

        this.Availability.Subscribe(this.ListenForAvailabilityChanges);
        this.Status.Subscribe(this.ListenForStatusChanges);
        this.PanicMode.Subscribe(this.ListenForPanicModeChanges);
    }

    /// <summary>
    /// Gets the unit's call availability observable.
    /// </summary>
    public FObservable<FUnitAvailability> Availability { get; private set; } = new(FUnitAvailability.NotReceiving);

    /// <summary>
    /// Gets the unit's status observable.
    /// </summary>
    public FObservable<string> Status { get; private set; } = new(string.Empty);

    /// <summary>
    /// Gets the unit's panic mode observable.
    /// </summary>
    public FObservable<bool> PanicMode { get; private set; } = new(false);

    /// <summary>
    /// Gets or sets the callsign.
    /// </summary>
    public string Callsign { get; set; }

    /// <summary>
    /// Gets the currently attached callout.
    /// </summary>
    public SimpleCallout Callout { get; internal set; }

    /// <summary>
    /// Gets the currently attached traffic stop.
    /// </summary>
    public TrafficStop TrafficStop { get; internal set; }

    /// <summary>
    /// Gets or sets the department id.
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Gets the players in the unit.
    /// </summary>
    public ReadOnlyCollection<FPlayer> Players => this._players.AsReadOnly();

    /// <summary>
    /// Gets or sets the callout of the unit.
    /// </summary>
    internal Callout InternalCallout { get; set; }

    /// <summary>
    /// Adds a player to the unit.
    /// </summary>
    /// <param name="player">The player to add.</param>
    internal void AddPlayer(FPlayer player)
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
    internal void RemovePlayer(FPlayer player)
    {
        this._players.RemoveAll(x => x.NetId == player.NetId);
        player.UnitCallsign.Value = string.Empty;
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

    private void ListenForPanicModeChanges(bool state)
    {
        foreach (var player in this._players)
        {
            player.GetUnderlyingPlayer().TriggerEvent(Common.Events.Player.UpdatePanicModeState, state);
        }
    }
}