using System;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Models;
using FivePD.Server.Models.Events;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides access to player data,
/// including respective events and methods
/// for modifying a player.
/// </summary>
public interface IPlayerDataService
{
    /// <summary>
    /// Fires when the duty status of a player has been changed.
    /// </summary>
    public event EventHandler<PlayerDutyStatusChangeEventArgs> PlayerDutyStatusChanged;

    /// <summary>
    /// Fires when a player's rank has been changed.
    /// </summary>
    public event EventHandler<PlayerRankChangeEventArgs> PlayerRankChanged;

    /// <summary>
    /// Fires when a player's division has been changed.
    /// </summary>
    public event EventHandler<PlayerDivisionChangeEventArgs> PlayerDivisionChanged;

    /// <summary>
    /// Finds the <see cref="User"/> object corresponding to the <paramref name="player"/>.
    /// </summary>
    /// <param name="player">The player to find.</param>
    /// <param name="withChangeTracking">Specifies whether to add AsNoTracking to the query or not.</param>
    /// <returns>The corresponding <see cref="User"/> object.</returns>
    public User FindUserByLicense(Player player, bool withChangeTracking = true);

    /// <summary>
    /// Checks whether any identifier of the specified player matches with any identifier of the specified user.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="user">The user.</param>
    /// <returns>A boolean.</returns>
    public bool AnyPlayerIdMatchesWithUserId(Player player, User user);

    /// <summary>
    /// Increases the rank of the specified player to the next rank with higher required experience points.
    /// In order to use this the ProgressionSystem feature flag must be enabled.
    /// </summary>
    /// <param name="player">The player.</param>
    public void IncreaseRank(FPlayer player);

    /// <summary>
    /// Decreases the rank of the specified player to the previous rank with lower required experience points.
    /// In order to use this the ProgressionSystem feature flag must be enabled.
    /// </summary>
    /// <param name="player">The player.</param>
    public void DecreaseRank(FPlayer player);
}