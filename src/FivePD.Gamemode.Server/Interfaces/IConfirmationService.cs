using System;
using System.Collections.Generic;
using CitizenFX.Core;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Provides methods for triggering a confirmation on the client.
/// </summary>
public interface IConfirmationService
{
    /// <summary>
    /// Displays a tooltip (if not null) and a notification (if not null) to the player and waits for a keypress,
    /// then responds with a boolean.
    /// </summary>
    /// <param name="player">The player to trigger the confirmation for.</param>
    /// <param name="key">The key to press. See full list here: https://docs.fivem.net/docs/game-references/controls.</param>
    /// <param name="tooltip">The tooltip to display. It'll be displayed only if not empty or null.</param>
    /// <param name="notification">The notification to display. It'll be displayed only if not empty or null.</param>
    /// <param name="notificationParams">The parameters to replace in the notification. "timeout" will be automatically added.</param>
    /// <param name="callback">The callback to invoke at response.</param>
    /// <param name="timeoutInMs">The timeout to wait for the keypress in milliseconds.</param>
    public void TriggerConfirmation(
        Player player,
        int key,
        string tooltip,
        string notification,
        Dictionary<string, string> notificationParams,
        Action<bool> callback,
        int timeoutInMs = 5000);
}