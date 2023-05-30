using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Client.Requests;
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
    public void ShowTooltip(Player player, string content, bool withBeep = false)
    {
        this._eventService.TriggerEventWithoutCallback(player, Events.Notification.ShowTooltip, JsonConvert.SerializeObject(new NotificationDTO.ShowTooltip
        {
            Content = content,
            WithBeep = withBeep,
        }));
    }

    /// <inheritdoc />
    public void ShowTooltip(Player player, GtaStyleText content, bool withBeep = false)
    {
        this.ShowTooltip(player, content.Build(), withBeep);
    }

    /// <inheritdoc />
    public void ShowNotification(Player player, string content)
    {
        this._eventService.TriggerEventWithoutCallback(player, Events.Notification.ShowNotification, JsonConvert.SerializeObject(new NotificationDTO.ShowNotification
        {
            Content = content,
        }));
    }

    /// <inheritdoc />
    public void ShowNotification(Player player, GtaStyleText content)
    {
        this.ShowNotification(player, content.Build());
    }

    /// <inheritdoc />
    public void ShowNotificationWithTexture(Player player, string texture, string sender, string subject, string content)
    {
        this._eventService.TriggerEventWithoutCallback(player, Events.Notification.ShowNotification, JsonConvert.SerializeObject(new NotificationDTO.ShowNotificationWithTexture
        {
            Texture = texture,
            Subject = subject,
            Sender = sender,
            Content = content,
        }));
    }

    /// <inheritdoc />
    public void ShowNotificationWithTexture(Player player, string texture, string sender, string subject, GtaStyleText content)
    {
        this.ShowNotificationWithTexture(player, texture, sender, subject, content.Build());
    }
}