using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FivePD.Gamemode.Server.EntityWrappers;

namespace FivePD.Gamemode.Server.Models;

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
    /// </summary>
    /// <param name="callsign">The callsign.</param>
    /// <param name="player">A player to add by default.</param>
    public Unit(string callsign, FPlayer player)
    {
        this.Callsign = callsign;
        this._players.Add(player);
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
    }

    /// <summary>
    /// Gets or sets the callsign.
    /// </summary>
    public string Callsign { get; set; }

    /// <summary>
    /// Gets the players in the unit.
    /// </summary>
    public ReadOnlyCollection<FPlayer> Players => this._players.AsReadOnly();

    /// <summary>
    /// Adds a player to the unit.
    /// </summary>
    /// <param name="player">The player to add.</param>
    public void AddPlayer(FPlayer player)
    {
        if (this._players.FirstOrDefault(x => x.NetId == player.NetId) is null)
        {
            this._players.Add(player);
        }
    }

    /// <summary>
    /// Removes the player from the unit.
    /// </summary>
    /// <param name="player">The player to remove.</param>
    public void RemovePlayer(FPlayer player)
    {
        this._players.RemoveAll(x => x.NetId == player.NetId);
    }
}