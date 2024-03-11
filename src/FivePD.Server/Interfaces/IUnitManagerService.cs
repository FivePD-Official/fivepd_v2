using System;
using System.Collections.Generic;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Models;
using FivePD.Server.Models.Events;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IUnitManagerService
{
    /// <summary>
    /// Invoked when a unit is created.
    /// </summary>
    public event EventHandler<Unit> OnUnitCreated;

    /// <summary>
    /// Invoked when a unit is removed.
    /// </summary>
    public event EventHandler<Unit> OnUnitRemoved;

    /// <summary>
    /// Invoked when a player joined a unit.
    /// </summary>
    public event EventHandler<UnitPlayerEventArgs> OnPlayerJoinedUnit;

    /// <summary>
    /// Invoked when a player left a unit.
    /// </summary>
    public event EventHandler<UnitPlayerEventArgs> OnPlayerLeftUnit;

    /// <summary>
    /// Creates a unit with the specified callsign and player.
    /// </summary>
    /// <param name="callsign">The callsign of the player.</param>
    /// <param name="player">A player to add by default.</param>
    /// <returns>The created unit.</returns>
    public Unit CreateUnit(string callsign, FPlayer player);

    /// <summary>
    /// Creates a unit with the specified callsign and player.
    /// </summary>
    /// <param name="callsign">The callsign of the player.</param>
    /// <param name="players">The players to add by default.</param>
    /// <returns>The created unit.</returns>
    public Unit CreateUnit(string callsign, IEnumerable<FPlayer> players);

    /// <summary>
    /// Gets the unit of the specified player.
    /// </summary>
    /// <param name="player">A player.</param>
    /// <returns>The unit of the player.</returns>
    public Unit GetPlayerUnit(FPlayer player);

    /// <summary>
    /// Adds the player to the given unit.
    /// </summary>
    /// <param name="player">A player.</param>
    /// <param name="unit">A unit.</param>
    public void AddPlayerToUnit(FPlayer player, Unit unit);

    /// <summary>
    /// Removes a player from their unit.
    /// </summary>
    /// <param name="player">A player.</param>
    public void RemovePlayerFromUnit(FPlayer player);

    /// <summary>
    /// Enables panic mode for the given unit.
    /// </summary>
    /// <param name="unit">The unit.</param>
    public void EnablePanicMode(Unit unit);

    /// <summary>
    /// Disables panic mode for the given unit.
    /// </summary>
    /// <param name="unit">The unit.</param>
    public void DisablePanicMode(Unit unit);
}