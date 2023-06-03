using System;
using CitizenFX.Core;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Provides access to player data,
/// including respective events and methods
/// for modifying a player.
/// </summary>
public interface IPlayerDataService
{
    /// <summary>
    /// Fires when a player's rank has been changed.
    /// </summary>
    public event EventHandler<Rank> PlayerRankChanged;

    /// <summary>
    /// Fires when a player's division has been changed.
    /// </summary>
    public event EventHandler<Division> PlayerDivisionChanged;

    /// <summary>
    /// Finds the <see cref="User"/> object corresponding to the <paramref name="player"/>.
    /// </summary>
    /// <param name="player">The player to find.</param>
    /// <returns>The corresponding <see cref="User"/> object.</returns>
    public User FindUserByLicense(Player player);

    /// <summary>
    /// Checks whether any identifier of the specified player matches with any identifier of the specified user.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="user">The user.</param>
    /// <returns>A boolean.</returns>
    public bool AnyPlayerIdMatchesWithUserId(Player player, User user);
}