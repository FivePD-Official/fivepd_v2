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

        /// <inheritdoc />
        public void RegisterEventHandlers()
        {
            Cfx.API.RegisterNuiCallbackType(NuiEventType.Close);
            this._eventHandlerDictionary[$"__cfx_nui:{NuiEventType.Close}"] += new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
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
            this.SendMessage(NuiEventType.Close);
            Cfx.API.SetNuiFocus(false, false);
            this._isNuiOpened = false;
        }

        /// <inheritdoc />
        [Keybind("fivepd_open_mdt", "B", "Open MDT")]
        public void OpenMdt()
        {
            this.Show(NuiEventType.Mdt);
        }

        /// <inheritdoc />
        [Keybind("fivepd_open_dutyMenu", "G", "Open duty menu")]
        public void OpenDutyMenu()
        {
            this.Show(NuiEventType.DutyMenu, "#", false, false);
        }

        /// <inheritdoc />
        [Keybind("fivepd_menu_up", "UP", "Move up in menu")]
        public void MenuControlUp()
        {
            this.SendMessage(NuiEventType.MenuUp);
        }

        /// <inheritdoc />
        [Keybind("fivepd_menu_down", "DOWN", "Move down in menu")]
        public void MenuControlDown()
        {
            this.SendMessage(NuiEventType.MenuDown);
        }

        /// <inheritdoc />
        [Keybind("fivepd_menu_left", "LEFT", "Move left in menu")]
        public void MenuControlLeft()
        {
            this.SendMessage(NuiEventType.MenuLeft);
        }

        /// <inheritdoc />
        [Keybind("fivepd_menu_right", "RIGHT", "Move right in menu")]
        public void MenuControlRight()
        {
            this.SendMessage(NuiEventType.MenuRight);
        }

        /// <inheritdoc />
        [Keybind("fivepd_menu_select", "RETURN", "Selects item in menu")]
        public void MenuControlSelect()
        {
            this.SendMessage(NuiEventType.MenuSelect);
        }

        /// <summary>
        /// Opens the test radial menu.
        /// </summary>
        /// <param name="menuHashcode">Hashcode of the menu.</param>
        public void ShowRadialMenu(string menuHashcode)
        {
            // Centers cursor
            Cfx.API.SetCursorLocation(0.5f, 0.5f);

            this.Show(NuiEventType.RadialMenu, menuHashcode);
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
            if (this._isNuiOpened && type != NuiEventType.Mdt)
            {
                this.Hide();
                return;
            }

            this._isNuiOpened = true;

            this.SendMessage(type, subtype);
            Cfx.API.SetNuiFocus(hasFocus, hasCursor);
        }

        /// <summary>
        /// Send control event to the NUI.
        /// </summary>
        /// <param name="type">The event's type.</param>
        private void SendMessage(string type)
        {
            Cfx.API.SendNuiMessage("{\"type\":\"" + type + "\"}");
        }

        /// <summary>
        /// Send control event to the NUI.
        /// </summary>
        /// <param name="type">The event's type.</param>
        /// <param name="subtype">The event's subtype. (used only for radial menus).</param>
        private void SendMessage(string type, string subtype)
        {
            Cfx.API.SendNuiMessage("{\"type\":\"" + type + "\", \"subtype\":\"" + subtype + "\"}");
        }

        /// <summary>
        /// The necessary event types used for communicating with the Nui.
        /// Should contains the same types as the Terminal project has.
        /// </summary>
        private abstract class NuiEventType
        {
            public const string Close = "close";

            public const string Mdt = "view:mdt";
            public const string DutyMenu = "view:duty_menu";
            public const string RadialMenu = "view:radial_menu";

            public const string MenuUp = "menu:control_up";
            public const string MenuDown = "menu:control_down";
            public const string MenuLeft = "menu:control_left";
            public const string MenuRight = "menu:control_right";
            public const string MenuSelect = "menu:control_select";
        }
    }
}