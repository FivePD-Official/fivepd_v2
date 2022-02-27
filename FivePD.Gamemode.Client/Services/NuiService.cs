// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Dynamic;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc cref="INuiService" />
    internal class NuiService : BaseScript, INuiService
    {
        /**
         * <summary>Initializes a new instance of the <see cref="NuiService"/> class.</summary>
         */
        public NuiService()
        {
            Cfx.API.RegisterNuiCallbackType("closeMdt");
            this.EventHandlers["__cfx_nui:closeMdt"] += new Action<ExpandoObject, CallbackDelegate>((body, resultCallback) =>
            {
                this.Hide();
            });
        }

        /**
         * <inheritdoc />
         */
        [Command("open")]
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