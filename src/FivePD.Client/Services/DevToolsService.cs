using System;
using System.Dynamic;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using Newtonsoft.Json;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IDevToolsService" />
internal class DevToolsService : IDevToolsService, IOnInit
{
    private readonly INotificationService _notificationService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="DevToolsService"/> class.
    /// </summary>
    /// <param name="notificationService">The <see cref="INotificationService"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public DevToolsService(INotificationService notificationService, EventHandlerDictionary eventHandlerDictionary)
    {
        this._notificationService = notificationService;
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.DevTools.DeleteEntities);
        this._eventHandlerDictionary.AddUIEvent(
            Events.DevTools.GetCurrentPlayerCoordinate,
            new Action<ExpandoObject, CallbackDelegate>(this.GetCurrentPlayerCoordinate));
    }

    /// <inheritdoc/>
    public void ShowDevelopmentMessage(string message)
    {
        this._notificationService.ShowNotificationWithTexture(
            "CHAR_DETONATEPHONE",
            "FivePD",
            "Development Message",
            message);
    }

    private void GetCurrentPlayerCoordinate(ExpandoObject expandoObject, CallbackDelegate callbackDelegate)
    {
        callbackDelegate.Invoke(JsonConvert.SerializeObject(Game.PlayerPed.Position));
    }
}