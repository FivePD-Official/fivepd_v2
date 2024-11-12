using System;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FivePD.Client.Services;

#pragma warning disable

internal class GameNotificationService : IGameNotificationService, IOnInit
{
    private readonly IUIManagerService _uiService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    public GameNotificationService(IUIManagerService uiService, EventHandlerDictionary eventHandlerDictionary)
    {
        this._uiService = uiService;
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    public void OnInit()
    {
        this._eventHandlerDictionary[Events.GameNotification.Notify] += new Action<string>(data => this._uiService.SendMessage(data));
    }

    public void Notify(GameNotificationOptions options)
    {
        var a = JsonConvert.SerializeObject(options);
        var b = JObject.Parse(a);
        b["Event"] = Events.GameNotification.Notify;
        this._uiService.SendMessage(b);
    }
}