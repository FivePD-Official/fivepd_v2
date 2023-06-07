using System.Collections.Generic;
using System.Collections.ObjectModel;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Provides methods for managing units.
/// </summary>
public interface IUnitService
{
    /// <summary>
    /// Gets the units.
    /// </summary>
    public ReadOnlyCollection<Unit> Units { get; }

    /// <summary>
    /// Creates a unit with the specified callsign and player.
    /// </summary>
    /// <param name="callsign">The callsign of the player.</param>
    /// <param name="player">A player to add by default.</param>
    /// <returns>The created unit.</returns>
    internal Unit CreateUnit(string callsign, FPlayer player);

    /// <summary>
    /// Creates a unit with the specified callsign and player.
    /// </summary>
    /// <param name="callsign">The callsign of the player.</param>
    /// <param name="players">The players to add by default.</param>
    /// <returns>The created unit.</returns>
    internal Unit CreateUnit(string callsign, IEnumerable<FPlayer> players);

    /// <summary>
    /// Gets the unit of the specified player.
    /// </summary>
    /// <param name="player">A player.</param>
    /// <returns>The unit of the player.</returns>
    public Unit GetPlayerUnit(FPlayer player);

    /// <summary>
    /// Removes a player from their unit.
    /// </summary>
    /// <param name="player">A player.</param>
    public void RemovePlayerFromUnit(FPlayer player);
}