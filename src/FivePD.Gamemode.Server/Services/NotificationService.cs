using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Common.DTO;
using FivePD.Gamemode.Common.Utilities;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services;

/// <inheritdoc />
public class NotificationService : INotificationService
{
    private readonly IEventService _eventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="eventService">The <see cref="IEventService"/> to use.</param>
    public NotificationService(IEventService eventService)
    {
        this._eventService = eventService;
    }

    /// <inheritdoc />
    public void ShowTooltip(Player player, string content, bool withBeep = false, Dictionary<string, string> contentParams = null)
    {
        this._eventService.TriggerEventWithoutCallback(player, Events.Notification.ShowTooltip, JsonConvert.SerializeObject(new NotificationDTO.ShowTooltip
        {
            Content = content,
            WithBeep = withBeep,
            ContentParams = contentParams ?? new Dictionary<string, string>(),
        }));
    }

    /// <inheritdoc />
    public void ShowTooltip(Player player, GtaStyleText content, bool withBeep = false, Dictionary<string, string> contentParams = null)
    {
        this.ShowTooltip(player, content.Build(), withBeep, contentParams);
    }

    /// <inheritdoc />
    public void ShowNotification(Player player, string content, Dictionary<string, string> contentParams = null)
    {
        this._eventService.TriggerEventWithoutCallback(player, Events.Notification.ShowNotification, JsonConvert.SerializeObject(
            new NotificationDTO.ShowNotification
            {
                Content = content,
                ContentParams = contentParams ?? new Dictionary<string, string>(),
            }));
    }

    /// <inheritdoc />
    public void ShowNotification(Player player, GtaStyleText content, Dictionary<string, string> contentParams = null)
    {
        this.ShowNotification(player, content.Build(), contentParams);
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
        this._eventService.TriggerEventWithoutCallback(player, Events.Notification.ShowNotification, JsonConvert.SerializeObject(
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
        Player player,
        string texture,
        string sender,
        string subject,
        GtaStyleText content,
        Dictionary<string, string> contentParams = null)
    {
        this.ShowNotificationWithTexture(player, texture, sender, subject, content.Build(), contentParams);
    }
}