using CitizenFX.Core;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for displaying NUI based notifications outside the MDT.
/// </summary>
public interface IGameNotificationService
{
    /// <summary>
    /// Display a notification.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="options">The options.</param>
    public void Notify(Player player, GameNotificationOptions options);

    /// <summary>
    /// Display a notification.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="options">The options.</param>
    public void Notify(FPlayer player, GameNotificationOptions options);
}