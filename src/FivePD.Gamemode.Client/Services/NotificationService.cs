using System;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Common.DTO;
using FivePD.Gamemode.Common.Utilities;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;
using CfxUI = CitizenFX.Core.UI;

namespace FivePD.Gamemode.Client.Services;

/// <inheritdoc cref="FivePD.Gamemode.Client.Interfaces.INotificationService" />
public class NotificationService : INotificationService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public NotificationService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Notification.ShowTooltip] += new Action<string>(serializedRequest =>
        {
            var request = JsonConvert.DeserializeObject<NotificationDTO.ShowTooltip>(serializedRequest);
            this.ShowTooltip(request.Content.ReplaceParams(request.ContentParams), request.WithBeep);
        });
        this._eventHandlerDictionary[Events.Notification.ShowNotification] += new Action<string>(serializedRequest =>
        {
            var request = JsonConvert.DeserializeObject<NotificationDTO.ShowNotification>(serializedRequest);
            this.ShowNotification(request.Content.ReplaceParams(request.ContentParams));
        });
        this._eventHandlerDictionary[Events.Notification.ShowNotificationWithTexture] += new Action<string>(serializedRequest =>
        {
            var request = JsonConvert.DeserializeObject<NotificationDTO.ShowNotificationWithTexture>(serializedRequest);
            this.ShowNotificationWithTexture(request.Texture, request.Sender, request.Subject, request.Content.ReplaceParams(request.ContentParams));
        });
    }

    /// <inheritdoc />
    public void ShowTooltip(string content, bool withBeep = false)
    {
        Cfx.API.SetTextComponentFormat("STRING");
        Cfx.API.AddTextComponentString(content);
        Cfx.API.EndTextCommandDisplayHelp(0, false, withBeep, -1);
    }

    /// <inheritdoc />
    public void ShowTooltip(GtaStyleText content, bool withBeep = false)
    {
        this.ShowTooltip(content.Build(), withBeep);
    }

    /// <inheritdoc />
    public void ShowNotification(string content)
    {
        CfxUI.Screen.ShowNotification(content);
    }

    /// <inheritdoc />
    public void ShowNotification(GtaStyleText content)
    {
        this.ShowNotification(content.Build());
    }

    /// <inheritdoc />
    public void ShowNotificationWithTexture(string texture, string sender, string subject, string content)
    {
        Cfx.API.SetNotificationTextEntry("STRING");
        Cfx.API.SetNotificationColorNext(4);
        Cfx.API.AddTextComponentString(content);
        Cfx.API.SetTextScale(0.5f, 0.5f);
        Cfx.API.SetNotificationMessage(texture, texture, false, 0, sender, subject);
        Cfx.API.DrawNotification(true, false);
    }

    /// <inheritdoc />
    public void ShowNotificationWithTexture(string texture, string sender, string subject, GtaStyleText content)
    {
        this.ShowNotificationWithTexture(texture, sender, subject, content.Build());
    }
}