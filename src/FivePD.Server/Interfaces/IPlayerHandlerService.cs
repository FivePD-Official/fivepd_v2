using CitizenFX.Core;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Represents a service for handling player and user objects.
/// </summary>
public interface IPlayerHandlerService
{
    /// <summary>
    /// Finds the <see cref="Player"/> object corresponding to the <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The player.</returns>
    public Player FindPlayerOfUser(User user);

    /// <summary>
    /// Finds the <see cref="User"/> object corresponding to the <paramref name="player"/>.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The user.</returns>
    public User FindUserOfPlayer(Player player);
}