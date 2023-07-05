using System;
using System.Dynamic;
using CitizenFX.Core;
using FivePD.Client.Attributes;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Utilities;
using FivePD.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable

internal class UIManagerService : IUIManagerService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ILoggerService _loggerService;

    private bool _ready;
    private bool _hasFocus;
    private bool _mdtOpen;
    private bool _informationOpen;

    /// <summary>Initializes a new instance of the <see cref="UIService"/> class.</summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="loggerService">The <see cref="ILoggerService" /> to use.</param>
    public UIManagerService(EventHandlerDictionary eventHandlerDictionary, ILoggerService loggerService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._loggerService = loggerService;
    }

    public bool MenuOpen { get; private set; }

    public void RegisterEvents()
    {
        // Listens for NUI status checks
        this._eventHandlerDictionary.AddUIEvent(
            Events.NuiEventType.ClientReady,
            new Action<ExpandoObject, CallbackDelegate>((_, cb) =>
            {
                this._loggerService.Info("Starting FivePD NUI...");
                this._ready = true;
                cb.Invoke(true);
            }));
    }
    
    /// <inheritdoc />
    public void SendMessage(string data)
    {
        Cfx.API.SendNuiMessage(data);
    }

    /// <inheritdoc />
    public void SendMessage(JObject data)
    {
        Cfx.API.SendNuiMessage(JsonConvert.SerializeObject(data));
    }

    /// <inheritdoc />
    public void Show(string type, string menuId = "")
    {
        if (!this._ready)
        {
            return;
        }

        if (type == Events.NuiEventType.Mdt)
        {
            if (this._mdtOpen)
            {
                this._hasFocus = false;
                this._mdtOpen = false;
                Cfx.API.SetNuiFocus(false, false);
            }
            else
            {
                this._hasFocus = true;
                this._mdtOpen = true;
                this.SendMessage(new JObject
                {
                    { "type", type },
                    { "menuId", string.Empty },
                });
                Cfx.API.SetNuiFocus(true, true);
            }

            return;
        }
        
        if (type == Events.NuiEventType.MenuView)
        {
            if (this.MenuOpen)
            {
                this._hasFocus = false;
                this.MenuOpen = false;
                this.SendMessage(new JObject
                {
                    { "type", type },
                    { "menuId", string.Empty },
                });
            }
            else
            {
                this._hasFocus = true;
                this.MenuOpen = true;
                this.SendMessage(new JObject
                {
                    { "type", type },
                    { "menuId", menuId },
                });
            }
            
            Cfx.API.SetNuiFocus(false, false);

            return;
        }
        
        if (type == Events.NuiEventType.Information)
        {
            if (this._informationOpen)
            {
                this._hasFocus = false;
                this._informationOpen = false;
            }
            else
            {
                this._hasFocus = true;
                this._informationOpen = true;
            }
            
            Cfx.API.SetNuiFocus(false, false);
            this.SendMessage(new JObject
            {
                { "type", type },
                { "menuId", string.Empty },
            });

            return;
        }
    }
    
    /// <inheritdoc />
    [Keybind(KeybindIdentifiers.MenuClose, "BACK", "Close menu")]
    public void Hide()
    {
        if (!this.MenuOpen)
        {
            return;
        }

        this._hasFocus = false;
        this.MenuOpen = false;
        Cfx.API.SetNuiFocus(false, false);
        this.SendMessage(new JObject
        {
            { "type", Events.NuiEventType.MenuView },
            { "menuId", string.Empty },
        });
    }

    public void Close()
    {
        this._hasFocus = false;
        this._mdtOpen = false;
        Cfx.API.SetNuiFocus(false, false);
    }

    /// <inheritdoc />
    public void ShowTimerBar(int timeToHoldInMs)
    {
        Cfx.API.SetNuiFocus(false, false);
        this.SendMessage(new JObject
        {
            { "type", Events.NuiEventType.TimerBar },
            { "show", true },
            { "title", "test" },
            { "timeToHold", timeToHoldInMs },
        });
    }
}