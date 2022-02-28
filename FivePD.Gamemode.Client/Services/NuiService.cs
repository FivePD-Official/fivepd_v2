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

        /// <summary>Initializes a new instance of the <see cref="NuiService"/> class.</summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>>
        public NuiService(EventHandlerDictionary eventHandlerDictionary)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
        }

        /// <summary>
        /// Registers all necessary NUI events for the MDT and menus.
        /// </summary>
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
        [Keybind("fivepd_open_mdt", "B")]
        public void Show()
        {
            Cfx.API.SendNuiMessage("{\"type\":\"open\"}");
            Cfx.API.SetNuiFocus(true, true);
        }

        /**
         * <inheritdoc />
         */
        public void Hide()
        {
            Cfx.API.SetNuiFocus(false, false);
        }
    }
}