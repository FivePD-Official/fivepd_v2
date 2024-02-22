using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides methods for drawing markers.
/// </summary>
public interface IMarkerService
{
    /// <summary>
    /// Upon invoking immediately starts drawing the specified marker for the specified player.
    /// </summary>
    /// <param name="player">The player to draw the marker for.</param>
    /// <param name="marker">The marker.</param>
    /// <returns>The id of the marker.</returns>
    public Guid AddMarker(Player player, Marker marker);

    /// <summary>
    /// Upon invoking immediately starts drawing the specified marker for the specified players.
    /// </summary>
    /// <param name="players">The players to draw the marker for.</param>
    /// <param name="marker">The marker.</param>
    /// <returns>The id of the marker.</returns>
    public Guid AddMarker(IEnumerable<Player> players, Marker marker);

    /// <summary>
    /// Stops drawing the marker with the specified id.
    /// </summary>
    /// <param name="guid">The id of the marker.</param>
    public void RemoveMarker(Guid guid);

    /// <summary>
    /// Returns a list of markers.
    /// </summary>
    /// <returns>A list of existing markers.</returns>
    public Dictionary<string, Player> GetMarkers();
}