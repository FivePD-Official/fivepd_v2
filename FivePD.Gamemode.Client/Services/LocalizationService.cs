// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Localization;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S1450

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class LocalizationService : ILocalizationService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly IResourceService _resourceService;
        private readonly string _defaultLocale = "en";

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
        /// <param name="resourceService">The <see cref="IResourceService" /> to use.</param>
        public LocalizationService(EventHandlerDictionary eventHandlerDictionary, IResourceService resourceService)
        {
            this.Locales = new List<Locale>();
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._resourceService = resourceService;
        }

        /// <inheritdoc />
        public List<Locale> Locales { get; }

        /// <inheritdoc />
        public Localization CurrentLocalization { get; private set; }

        /// <inheritdoc />
        public void RegisterEventHandlers()
        {
            this._eventHandlerDictionary.AddNuiEvent(Events.Localization.GetLocalizationFromNui, new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
            {
                cb.Invoke(this.CurrentLocalization);
            }));
        }

        /// <inheritdoc />
        public async Task<bool> LoadAvailableLocales()
        {
            int hasServerResponded = 0;
            var now = DateTime.Now;
            var timeUntilFinishWait = now.AddMinutes(1);

            BaseScript.TriggerServerEvent(Events.Localization.GetAvailableLocalesFromServer, new Action<string>(locales =>
            {
                hasServerResponded = 1;
                foreach (var item in JArray.Parse(locales))
                {
                    this.Locales.Add(new Locale(item["Key"].ToString(), item["Title"].ToString()));
                }
            }));

            do
            {
                if (DateTime.Compare(DateTime.Now, timeUntilFinishWait) > 0)
                {
                    return false;
                }

                await BaseScript.Delay(1);
            }
            while (hasServerResponded == 0);

            return true;
        }

        /// <inheritdoc />
        public void LoadDefaultLocalization()
        {
            this.LoadLocalization(this._defaultLocale);
        }

        /// <inheritdoc />
        public void LoadLocalization(string locale)
        {
            this.CurrentLocalization = this._resourceService.LoadFile<Localization>($"/Localization/{locale}.json");
        }
    }
}