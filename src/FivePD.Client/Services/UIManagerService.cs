using System;
using System.Dynamic;
using CitizenFX.Core;
using FivePD.Client.Attributes;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Client.Utilities;
using FivePD.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable

internal class UIManagerService : IUIManagerService, IOnInit
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

    public bool WelcomeModalOpen { get; private set; }

    public void OnInit()
    {
        // Listens for NUI status checks
        this._eventHandlerDictionary.AddUIEvent(
            Events.UIEvent.ClientReady,
            new Action<ExpandoObject, CallbackDelegate>((_, cb) =>
            {
                this._loggerService.Info("FivePD UI is ready");

                this._hasFocus = false;
                Cfx.API.SetNuiFocus(false, false);

                this._ready = true;

                this._mdtOpen = false;
                this._informationOpen = false;
                this.WelcomeModalOpen = false;
                this.MenuOpen = false;
                
                // Show welcome modal for first login
                BaseScript.TriggerServerEvent(Events.Player.GetDismissedWelcomeModal, new Action<bool>(dismissedWelcomeModal =>
                {
                    if (dismissedWelcomeModal)
                    {
                        return;
                    }

                    this.ToggleView(Events.UIEvent.WelcomeModal);
                }));

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
    public void ToggleView(string type, string menuId = "")
    {
        if (!this._ready)
        {
            return;
        }

        if (type == Events.UIEvent.Mdt)
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
                    { "Event", type },
                    { "MenuId", string.Empty },
                });
                Cfx.API.SetNuiFocus(true, true);
            }

            return;
        }

        if (type == Events.UIEvent.MenuView)
        {
            if (this.MenuOpen)
            {
                this._hasFocus = false;
                this.MenuOpen = false;
                this.SendMessage(new JObject
                {
                    { "Event", type },
                    { "MenuId", string.Empty },
                });
            }
            else
            {
                this._hasFocus = true;
                this.MenuOpen = true;
                this.SendMessage(new JObject
                {
                    { "Event", type },
                    { "MenuId", menuId },
                });
            }

            Cfx.API.SetNuiFocus(false, false);

            return;
        }

        if (type == Events.UIEvent.Information)
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
                { "Event", type },
                { "MenuId", string.Empty },
            });
        }

        if (type == Events.UIEvent.WelcomeModal)
        {
            if (this.WelcomeModalOpen)
            {
                this._hasFocus = false;
                this.WelcomeModalOpen = false;
            }
            else
            {
                this._hasFocus = true;
                this.WelcomeModalOpen = true;
            }

            Cfx.API.SetNuiFocus(false, false);
            this.SendMessage(new JObject
            {
                { "Event", type },
                { "MenuId", string.Empty },
            });
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
            { "Event", Events.UIEvent.MenuView },
            { "MenuId", string.Empty },
        });
    }

    public void Close()
    {
        this._hasFocus = false;
        this._mdtOpen = false;
        this.MenuOpen = false;
        Cfx.API.SetNuiFocus(false, false);
    }

    /// <inheritdoc />
    public void ShowTimerBar(int timeToHoldInMs)
    {
        Cfx.API.SetNuiFocus(false, false);
        this.SendMessage(new JObject
        {
            { "Event", Events.UIEvent.TimerBar },
            { "Show", true },
            { "Title", "test" },
            { "TimeToHold", timeToHoldInMs },
        });
    }
}