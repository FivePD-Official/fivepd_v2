using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class NotificationService : INotificationService
{
    /// <inheritdoc />
    public void ShowTooltip(Player player, string content, bool withBeep = false, int duration = -1)
    {
        player.TriggerEvent(Events.Notification.ShowTooltip, JsonConvert.SerializeObject(new NotificationDTO.ShowTooltip
        {
            Content = content,
            WithBeep = withBeep,
            Duration = duration,
        }));
    }

    /// <inheritdoc />
    public void ShowTooltip(FPlayer player, string content, bool withBeep = false, int duration = -1)
    {
        this.ShowTooltip(player.GetUnderlyingPlayer(), content, withBeep, duration);
    }

    /// <inheritdoc />
    public void ShowNotification(Player player, string content)
    {
        player.TriggerEvent(Events.Notification.ShowNotification, JsonConvert.SerializeObject(
            new NotificationDTO.ShowNotification
            {
                Content = content,
            }));
    }

    /// <inheritdoc />
    public void ShowNotification(FPlayer player, string content)
    {
        this.ShowNotification(player.GetUnderlyingPlayer(), content);
    }

    /// <inheritdoc />
    public void ShowNotification(IEnumerable<Player> players, string content)
    {
        foreach (var player in players)
        {
            this.ShowNotification(player, content);
        }
    }

    /// <inheritdoc />
    public void ShowNotification(IEnumerable<FPlayer> players, string content)
    {
        this.ShowNotification(players.Select(player => player.GetUnderlyingPlayer()), content);
    }

    /// <inheritdoc />
    public void ShowNotificationWithTexture(
        Player player,
        string texture,
        string sender,
        string subject,
        string content)
    {
        player.TriggerEvent(Events.Notification.ShowNotificationWithTexture, JsonConvert.SerializeObject(
            new NotificationDTO.ShowNotificationWithTexture
            {
                Texture = texture,
                Subject = subject,
                Sender = sender,
                Content = content,
            }));
    }

    /// <inheritdoc />
    public void ShowNotificationWithTexture(
        FPlayer player,
        string texture,
        string sender,
        string subject,
        string content)
    {
        this.ShowNotificationWithTexture(player.GetUnderlyingPlayer(), texture, sender, subject, content);
    }

    /// <inheritdoc />
    public void ShowNotificationWithTexture(
        IEnumerable<Player> players,
        string texture,
        string sender,
        string subject,
        string content)
    {
        foreach (var player in players)
        {
            this.ShowNotificationWithTexture(player, texture, sender, subject, content);
        }
    }

    /// <inheritdoc />
    public void ShowNotificationWithTexture(
        IEnumerable<FPlayer> players,
        string texture,
        string sender,
        string subject,
        string content)
    {
        this.ShowNotificationWithTexture(players.Select(player => player.GetUnderlyingPlayer()), texture, sender, subject, content);
    }
}