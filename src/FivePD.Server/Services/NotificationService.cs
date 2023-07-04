using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.DTO;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class NotificationService : INotificationService
{
    /// <inheritdoc />
    public void ShowTooltip(Player player, string content, bool withBeep = false, Dictionary<string, string> contentParams = null, int duration = -1)
    {
        player.TriggerEvent(Events.Notification.ShowTooltip, JsonConvert.SerializeObject(new NotificationDTO.ShowTooltip
        {
            Content = content,
            WithBeep = withBeep,
            ContentParams = contentParams ?? new Dictionary<string, string>(),
            Duration = duration,
        }));
    }

    /// <inheritdoc />
    public void ShowTooltip(FPlayer player, string content, bool withBeep = false, Dictionary<string, string> contentParams = null, int duration = -1)
    {
        this.ShowTooltip(player.GetUnderlyingPlayer(), content, withBeep, contentParams, duration);
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
    public void ShowNotification(FPlayer player, string content, Dictionary<string, string> contentParams = null)
    {
        this.ShowNotification(player.GetUnderlyingPlayer(), content, contentParams);
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
    public void ShowNotification(IEnumerable<FPlayer> players, string content, Dictionary<string, string> contentParams = null)
    {
        this.ShowNotification(players.Select(player => player.GetUnderlyingPlayer()), content, contentParams);
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
        FPlayer player,
        string texture,
        string sender,
        string subject,
        string content,
        Dictionary<string, string> contentParams = null)
    {
        this.ShowNotificationWithTexture(player.GetUnderlyingPlayer(), texture, sender, subject, content, contentParams);
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

    /// <inheritdoc />
    public void ShowNotificationWithTexture(
        IEnumerable<FPlayer> players,
        string texture,
        string sender,
        string subject,
        string content,
        Dictionary<string, string> contentParams = null)
    {
        this.ShowNotificationWithTexture(players.Select(player => player.GetUnderlyingPlayer()), texture, sender, subject, content, contentParams);
    }
}