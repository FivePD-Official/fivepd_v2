using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.DTO;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services;

/// <inheritdoc />
public class NotificationService : INotificationService
{
    /// <inheritdoc />
    public void ShowTooltip(Player player, string content, bool withBeep = false, Dictionary<string, string> contentParams = null)
    {
        player.TriggerEvent(Events.Notification.ShowTooltip, JsonConvert.SerializeObject(new NotificationDTO.ShowTooltip
        {
            Content = content,
            WithBeep = withBeep,
            ContentParams = contentParams ?? new Dictionary<string, string>(),
        }));
    }

    /// <inheritdoc />
    public void ShowNotification(Player player, string content, Dictionary<string, string> contentParams = null)
    {
        player.TriggerEvent(Events.Notification.ShowNotification, JsonConvert.SerializeObject(
            new NotificationDTO.ShowNotification
            {
                Content = content,
                ContentParams = contentParams ?? new Dictionary<string, string>(),
            }));
    }

    /// <inheritdoc />
    public void ShowNotification(IEnumerable<Player> players, string content, Dictionary<string, string> contentParams = null)
    {
        foreach (var player in players)
        {
            this.ShowNotification(player, content, contentParams);
        }
    }

    /// <inheritdoc />
    public void ShowNotificationWithTexture(
        Player player,
        string texture,
        string sender,
        string subject,
        string content,
        Dictionary<string, string> contentParams = null)
    {
        player.TriggerEvent(Events.Notification.ShowNotificationWithTexture, JsonConvert.SerializeObject(
            new NotificationDTO.ShowNotificationWithTexture
            {
                Texture = texture,
                Subject = subject,
                Sender = sender,
                Content = content,
                ContentParams = contentParams ?? new Dictionary<string, string>(),
            }));
    }

    /// <inheritdoc />
    public void ShowNotificationWithTexture(
        IEnumerable<Player> players,
        string texture,
        string sender,
        string subject,
        string content,
        Dictionary<string, string> contentParams = null)
    {
        foreach (var player in players)
        {
            this.ShowNotificationWithTexture(player, texture, sender, subject, content, contentParams);
        }
    }
}