using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for displaying base GTA notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Displays a tooltip (small notification in the top-left corner) for the specified player.
    /// </summary>
    /// <param name="player">The player which the tooltip will be displayed for.</param>
    /// <param name="content">The text that'll be displayed.</param>
    /// <param name="withBeep">If the notification should play a beep sound.</param>
    /// <param name="contentParams">The parameters to be replaced in the content.</param>
    /// <param name="duration">The duration in ms.</param>
    public void ShowTooltip(Player player, string content, bool withBeep = false, Dictionary<string, string> contentParams = null, int duration = -1);

    /// <summary>
    /// Displays a tooltip (small notification in the top-left corner) for the specified player.
    /// </summary>
    /// <param name="player">The player which the tooltip will be displayed for.</param>
    /// <param name="content">The text that'll be displayed.</param>
    /// <param name="withBeep">If the notification should play a beep sound.</param>
    /// <param name="contentParams">The parameters to be replaced in the content.</param>
    /// <param name="duration">The duration in ms.</param>
    public void ShowTooltip(FPlayer player, string content, bool withBeep = false, Dictionary<string, string> contentParams = null, int duration = -1);

    /// <summary>
    /// Displays a notification for the specified player.
    /// </summary>
    /// <param name="player">The player which the notification will be displayed for.</param>
    /// <param name="content">The text that'll be displayed.</param>
    /// <param name="contentParams">The parameters to be replaced in the content.</param>
    public void ShowNotification(Player player, string content, Dictionary<string, string> contentParams = null);

    /// <summary>
    /// Displays a notification for the specified player.
    /// </summary>
    /// <param name="player">The player which the notification will be displayed for.</param>
    /// <param name="content">The text that'll be displayed.</param>
    /// <param name="contentParams">The parameters to be replaced in the content.</param>
    public void ShowNotification(FPlayer player, string content, Dictionary<string, string> contentParams = null);

    /// <summary>
    /// Displays a notification for the specified players.
    /// </summary>
    /// <param name="players">The players which the notification will be displayed for.</param>
    /// <param name="content">The text that'll be displayed.</param>
    /// <param name="contentParams">The parameters to be replaced in the content.</param>
    public void ShowNotification(IEnumerable<Player> players, string content, Dictionary<string, string> contentParams = null);

    /// <summary>
    /// Displays a notification for the specified players.
    /// </summary>
    /// <param name="players">The players which the notification will be displayed for.</param>
    /// <param name="content">The text that'll be displayed.</param>
    /// <param name="contentParams">The parameters to be replaced in the content.</param>
    public void ShowNotification(IEnumerable<FPlayer> players, string content, Dictionary<string, string> contentParams = null);

    /// <summary>
    /// Displays a notification with a texture for the specified player.
    /// </summary>
    /// <param name="player">The player which the notification will be displayed for.</param>
    /// <param name="texture">The texture's name which is the top-left image in the notification.</param>
    /// <param name="sender">The sender of the notification, will be displayed on the first line.</param>
    /// <param name="subject">The subject of the notification, will be displayed on the second line.</param>
    /// <param name="content">The text that'll be displayed.</param>
    /// <param name="contentParams">The parameters to be replaced in the content.</param>
    public void ShowNotificationWithTexture(
        Player player,
        string texture,
        string sender,
        string subject,
        string content,
        Dictionary<string, string> contentParams = null);

    /// <summary>
    /// Displays a notification with a texture for the specified player.
    /// </summary>
    /// <param name="player">The player which the notification will be displayed for.</param>
    /// <param name="texture">The texture's name which is the top-left image in the notification.</param>
    /// <param name="sender">The sender of the notification, will be displayed on the first line.</param>
    /// <param name="subject">The subject of the notification, will be displayed on the second line.</param>
    /// <param name="content">The text that'll be displayed.</param>
    /// <param name="contentParams">The parameters to be replaced in the content.</param>
    public void ShowNotificationWithTexture(
        FPlayer player,
        string texture,
        string sender,
        string subject,
        string content,
        Dictionary<string, string> contentParams = null);

    /// <summary>
    /// Displays a notification with a texture for the specified players.
    /// </summary>
    /// <param name="players">The player which the notification will be displayed for.</param>
    /// <param name="texture">The texture's name which is the top-left image in the notification.</param>
    /// <param name="sender">The sender of the notification, will be displayed on the first line.</param>
    /// <param name="subject">The subject of the notification, will be displayed on the second line.</param>
    /// <param name="content">The text that'll be displayed.</param>
    /// <param name="contentParams">The parameters to be replaced in the content.</param>
    public void ShowNotificationWithTexture(
        IEnumerable<Player> players,
        string texture,
        string sender,
        string subject,
        string content,
        Dictionary<string, string> contentParams = null);

    /// <summary>
    /// Displays a notification with a texture for the specified players.
    /// </summary>
    /// <param name="players">The player which the notification will be displayed for.</param>
    /// <param name="texture">The texture's name which is the top-left image in the notification.</param>
    /// <param name="sender">The sender of the notification, will be displayed on the first line.</param>
    /// <param name="subject">The subject of the notification, will be displayed on the second line.</param>
    /// <param name="content">The text that'll be displayed.</param>
    /// <param name="contentParams">The parameters to be replaced in the content.</param>
    public void ShowNotificationWithTexture(
        IEnumerable<FPlayer> players,
        string texture,
        string sender,
        string subject,
        string content,
        Dictionary<string, string> contentParams = null);
}