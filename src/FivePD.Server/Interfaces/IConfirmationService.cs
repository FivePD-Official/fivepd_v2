using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

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
    /// <param name="properties">The properties of the confirmation notifications.</param>
    /// <param name="callback">The callback to invoke at response.</param>
    /// <param name="timeoutInMs">The timeout to wait for the keypress in milliseconds.</param>
    public void TriggerConfirmation(
        Player player,
        ConfirmationProperties properties,
        Action<bool> callback,
        int timeoutInMs = 5000);

    /// <summary>
    /// Displays a tooltip (if not null) and a notification (if not null) to the player and waits for a keypress,
    /// then responds with a boolean.
    /// </summary>
    /// <param name="player">The player to trigger the confirmation for.</param>
    /// <param name="properties">The properties of the confirmation notifications.</param>
    /// <param name="callback">The callback to invoke at response.</param>
    /// <param name="timeoutInMs">The timeout to wait for the keypress in milliseconds.</param>
    public void TriggerConfirmation(
        FPlayer player,
        ConfirmationProperties properties,
        Action<bool> callback,
        int timeoutInMs = 5000);

    /// <summary>
    /// Displays a tooltip (if not null) and a notification (if not null) to the players of the unit and waits for a keypress,
    /// then responds with a boolean.
    /// </summary>
    /// <param name="unit">The unit to trigger the confirmation for.</param>
    /// <param name="properties">The properties of the confirmation notifications.</param>
    /// <param name="callback">The callback to invoke at response.</param>
    /// <param name="timeoutInMs">The timeout to wait for the keypress in milliseconds.</param>
    public void TriggerConfirmation(
        Unit unit,
        ConfirmationProperties properties,
        Action<bool> callback,
        int timeoutInMs = 5000);

    /// <summary>
    /// Displays a tooltip (if not null) and a notification (if not null) to the player and waits for a keypress,
    /// then responds with a boolean.
    /// </summary>
    /// <param name="player">The player to trigger the confirmation for.</param>
    /// <param name="properties">The properties of the confirmation notifications.</param>
    /// <param name="timeoutInMs">The timeout to wait for the keypress in milliseconds.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<bool> TriggerConfirmationAsync(
        Player player,
        ConfirmationProperties properties,
        int timeoutInMs = 5000);

    /// <summary>
    /// Displays a tooltip (if not null) and a notification (if not null) to the player and waits for a keypress,
    /// then responds with a boolean.
    /// </summary>
    /// <param name="player">The player to trigger the confirmation for.</param>
    /// <param name="properties">The properties of the confirmation notifications.</param>
    /// <param name="timeoutInMs">The timeout to wait for the keypress in milliseconds.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<bool> TriggerConfirmationAsync(
        FPlayer player,
        ConfirmationProperties properties,
        int timeoutInMs = 5000);
}