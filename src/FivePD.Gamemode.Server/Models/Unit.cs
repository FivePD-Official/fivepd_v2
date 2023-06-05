using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FivePD.Gamemode.Server.EntityWrappers;

namespace FivePD.Gamemode.Server.Models;

#pragma warning disable

public class Unit
{
    public string Callsign { get; set; }

    private readonly List<FPlayer> _players = new();

    public ReadOnlyCollection<FPlayer> Players => this._players.AsReadOnly();

    public Unit(string callsign)
    {
        this.Callsign = callsign;
    }

    public Unit(string callsign, FPlayer player)
    {
        this.Callsign = callsign;
        this._players.Add(player);
    }

    public Unit(string callsign, IEnumerable<FPlayer> players)
    {
        this.Callsign = callsign;
        this._players.AddRange(players);
    }

    public void AddPlayer(FPlayer player)
    {
        if (this._players.FirstOrDefault(x => x.NetId == player.NetId) is null)
        {
            this._players.Add(player);
        }
    }

    public void RemovePlayer(FPlayer player)
    {
        this._players.RemoveAll(x => x.NetId == player.NetId);
    }
}