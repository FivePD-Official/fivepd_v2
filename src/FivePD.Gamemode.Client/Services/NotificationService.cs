using System;
using System.Collections.Generic;
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
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
    public NotificationService(EventHandlerDictionary eventHandlerDictionary, ILocalizationService localizationService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._localizationService = localizationService;
    }

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Notification.ShowTooltip] += new Action<string>(serializedRequest =>
        {
            var request = JsonConvert.DeserializeObject<NotificationDTO.ShowTooltip>(serializedRequest);
            this.ShowTooltip(this.GetLocalizedContent(request.Content, request.ContentParams), request.WithBeep);
        });
        this._eventHandlerDictionary[Events.Notification.ShowNotification] += new Action<string>(serializedRequest =>
        {
            var request = JsonConvert.DeserializeObject<NotificationDTO.ShowNotification>(serializedRequest);
            this.ShowNotification(this.GetLocalizedContent(request.Content, request.ContentParams));
        });
        this._eventHandlerDictionary[Events.Notification.ShowNotificationWithTexture] += new Action<string>(serializedRequest =>
        {
            var request = JsonConvert.DeserializeObject<NotificationDTO.ShowNotificationWithTexture>(serializedRequest);
            this.ShowNotificationWithTexture(
                request.Texture,
                this.GetLocalizedText(request.Sender),
                this.GetLocalizedText(request.Subject),
                this.GetLocalizedContent(request.Content, request.ContentParams));
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

    private string GetLocalizedContent(string content, Dictionary<string, string> contentParams)
    {
        var localizedContent = this._localizationService.Get(content);
        var localizedParams = new Dictionary<string, string>();
        foreach (var param in contentParams)
        {
            var localizedParam = this._localizationService.Get(param.Value);
            localizedParams[param.Key] = string.IsNullOrEmpty(localizedParam) ? param.Value : localizedParam;
        }

        return string.IsNullOrEmpty(localizedContent) ? content : localizedContent.ReplaceParams(localizedParams);
    }

    private string GetLocalizedText(string text)
    {
        var localizedText = this._localizationService.Get(text);
        return string.IsNullOrEmpty(localizedText) ? text : localizedText;
    }
}