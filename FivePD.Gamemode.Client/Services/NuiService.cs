// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Dynamic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Client.Attributes;
using FivePD.Gamemode.Client.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc cref="INuiService" />
    public class NuiService : INuiService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private bool _isNuiOpened;

        /// <summary>Initializes a new instance of the <see cref="NuiService"/> class.</summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>>
        public NuiService(EventHandlerDictionary eventHandlerDictionary)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
        }

        /// <summary>
        /// Delegate for event <see cref="OnNuiHide"/>.
        /// </summary>
        public delegate void OnHide();

        /// <summary>
        /// Triggers when the Nui has been hidden.
        /// </summary>
        public event OnHide OnNuiHide;

        /// <inheritdoc />
        public void RegisterEventHandlers()
        {
            Cfx.API.RegisterNuiCallbackType(Events.NuiEventType.Close);
            this._eventHandlerDictionary[$"__cfx_nui:{Events.NuiEventType.Close}"] += new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
            {
                this.Hide();
                cb.Invoke(string.Empty);
            });

            Cfx.API.RegisterNuiCallbackType(Events.Nui.GetLoadout);
            this._eventHandlerDictionary[$"__cfx_nui:{Events.Nui.GetLoadout}"] += new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
            {
                Debug.WriteLine("Get loadout event from Nui.");
                cb.Invoke("Test response");
            });
        }

        /// <inheritdoc />
        [Keybind("fivepd_close_all", "BACK", "Close menu")]
        public void Hide()
        {
            this.SendMessage("{\"type\": \"" + Events.NuiEventType.Close + "\"}");
            Cfx.API.SetNuiFocus(false, false);
            this._isNuiOpened = false;
            this.OnNuiHide?.Invoke();
        }

        /// <inheritdoc />
        [Keybind("fivepd_open_mdt", "B", "Open MDT")]
        public void OpenMdt()
        {
            this.Show(Events.NuiEventType.Mdt);
        }

        /// <inheritdoc />
        [Keybind("fivepd_open_dutyMenu", "G", "Open duty menu")]
        public void OpenDutyMenu()
        {
            this.Show(Events.NuiEventType.DutyMenu, "#", false, false);
        }

        /// <inheritdoc />
        [Keybind("fivepd_menu_up", "UP", "Move up in menu")]
        public void MenuControlUp()
        {
            this.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuUp + "\"}");
        }

        /// <inheritdoc />
        [Keybind("fivepd_menu_down", "DOWN", "Move down in menu")]
        public void MenuControlDown()
        {
            this.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuDown + "\"}");
        }

        /// <inheritdoc />
        [Keybind("fivepd_menu_left", "LEFT", "Move left in menu")]
        public void MenuControlLeft()
        {
            this.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuLeft + "\"}");
        }

        /// <inheritdoc />
        [Keybind("fivepd_menu_right", "RIGHT", "Move right in menu")]
        public void MenuControlRight()
        {
            this.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuRight + "\"}");
        }

        /// <inheritdoc />
        [Keybind("fivepd_menu_select", "RETURN", "Selects item in menu")]
        public void MenuControlSelect()
        {
            this.SendMessage("{\"type\": \"" + Events.NuiEventType.MenuSelect + "\"}");
        }

        /// <inheritdoc />
        public void ShowRadialMenu(string menuId)
        {
            this.Show(Events.NuiEventType.RadialMenu, menuId, false, false);
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

        /// <summary>
        /// Shows NUI. If the MDT has been opened while any menu is open,
        /// the NUI view will be switched to the MDT.
        /// </summary>
        /// <param name="type">Switches to the given type after showing NUI.</param>
        /// <param name="subtype">The subtype of the event. If left as # it'll be ignore in the Nui.</param>
        /// <param name="hasFocus">If NUI has focus.</param>
        /// <param name="hasCursor">If NUI has cursor usage.</param>
        private void Show(string type, string subtype = "#", bool hasFocus = true, bool hasCursor = true)
        {
            if (this._isNuiOpened && type != Events.NuiEventType.Mdt)
            {
                this.Hide();
                return;
            }

            this._isNuiOpened = true;

            this.SendMessage(new JObject
            {
                { "type", type },
                { "subtype", subtype },
            });
            Cfx.API.SetNuiFocus(hasFocus, hasCursor);
        }
    }
}