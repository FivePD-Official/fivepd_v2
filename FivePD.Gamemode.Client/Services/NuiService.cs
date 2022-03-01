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
            Cfx.API.RegisterNuiCallbackType(Events.Nui.Close);
            this._eventHandlerDictionary[$"__cfx_nui:{Events.Nui.Close}"] += new Action<ExpandoObject, CallbackDelegate>((body, resultCallback) =>
            {
                this.Hide();
            });
        }

        /**
         * <inheritdoc />
         */
        [Keybind("fivepd_close_all", "BACK", "Close menu")]
        public void Hide()
        {
            this.SendMessage("close");
            Cfx.API.SetNuiFocus(false, false);
            this._isNuiOpened = false;
        }

        /// <summary>
        /// Opens the MDT.
        /// </summary>
        [Keybind("fivepd_open_mdt", "B", "Open MDT")]
        public void OpenMdt()
        {
            this.Show("mdt");
        }

        /// <summary>
        /// Opens the duty menu.
        /// </summary>
        [Keybind("fivepd_open_dutyMenu", "G", "Open duty menu")]
        public void OpenDutyMenu()
        {
            this.Show("duty_menu", false, false);
        }

        /// <summary>
        /// Moves up by one item in the currently opened menu.
        /// </summary>
        [Keybind("fivepd_menu_up", "UP", "Move up in menu")]
        public void MenuControlUp()
        {
            this.SendMessage("control_up");
        }

        /// <summary>
        /// Moves down by one item in the currently opened menu.
        /// </summary>
        [Keybind("fivepd_menu_down", "DOWN", "Move down in menu")]
        public void MenuControlDown()
        {
            this.SendMessage("control_down");
        }

        /// <summary>
        /// Selects the currently selected item in the menu.
        /// </summary>
        [Keybind("fivepd_menu_select", "RETURN", "Selects item in menu")]
        public void MenuSelect()
        {
            this.SendMessage("control_select");
        }

        /// <summary>
        /// Shows NUI.
        /// </summary>
        /// <param name="type">Switches to the given type after showing NUI.</param>
        /// <param name="hasFocus">If NUI has focus.</param>
        /// <param name="hasCursor">If NUI has cursor usage.</param>
        private void Show(string type, bool hasFocus = true, bool hasCursor = true)
        {
            if (this._isNuiOpened && type != "mdt")
            {
                this.Hide();
                return;
            }

            this._isNuiOpened = true;

            this.SendMessage(type);
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
    }
}