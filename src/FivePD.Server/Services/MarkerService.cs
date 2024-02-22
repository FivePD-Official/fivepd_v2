using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class MarkerService : IMarkerService
{
    private readonly PlayerList _playerList;

    private readonly Dictionary<string, Player> _markers = new Dictionary<string, Player>();

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkerService"/> class.
    /// </summary>
    /// <param name="playerList">The <see cref="PlayerList"/> to use.</param>
    public MarkerService(PlayerList playerList)
    {
        this._playerList = playerList;
    }

    /// <inheritdoc />
    public Guid AddMarker(Player player, Marker marker)
    {
        var guid = Guid.NewGuid();
        this.AddMarker(guid, marker, player);
        return guid;
    }

    /// <inheritdoc />
    public Guid AddMarker(IEnumerable<Player> players, Marker marker)
    {
        var guid = Guid.NewGuid();
        foreach (var player in players)
        {
            this.AddMarker(guid, marker, player);
        }

        return guid;
    }

    /// <inheritdoc />
    public void RemoveMarker(Guid guid)
    {
        var id = guid.ToString();
        if (!this._markers.ContainsKey(id))
        {
            return;
        }

        var player = this._markers[id];
        if (player is null)
        {
            foreach (var p in this._playerList)
            {
                p.TriggerEvent(Events.Marker.Remove, id);
            }
        }
        else
        {
            player.TriggerEvent(Events.Marker.Remove, id);
        }

        this._markers.Remove(id);
    }

    private void AddMarker(Guid guid, Marker marker, Player player)
    {
        var id = guid.ToString();
        this._markers.Add(id, player);
        player.TriggerEvent(Events.Marker.Add, JsonConvert.SerializeObject(new MarkerDTO.Add(id, marker)));
    }

    /// <inheritdoc />
    public Dictionary<string, Player> GetMarkers()
    {
        return this._markers;
    }
}