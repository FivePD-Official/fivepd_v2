// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Schemas;
using FivePD.Gamemode.Client.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class LocalizationService : ILocalizationService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly string _defaultLocale = "en";
        private readonly List<Locale> _locales;
        private Localization _currentLocalization;
#pragma warning disable S1450
        private string _currentLocale = "en";
#pragma warning restore S1450

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>>
        public LocalizationService(EventHandlerDictionary eventHandlerDictionary)
        {
            this._locales = new List<Locale>();
            this._eventHandlerDictionary = eventHandlerDictionary;
        }

        /// <inheritdoc />
        public void RegisterEventHandlers()
        {
            Cfx.API.RegisterNuiCallbackType(Events.Localization.GetLocalization);
            this._eventHandlerDictionary[$"__cfx_nui:{Events.Localization.GetLocalization}"] += new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
            {
                cb.Invoke(JsonConvert.SerializeObject(this._currentLocalization));
            });

            Cfx.API.RegisterNuiCallbackType(Events.Localization.GetAvailableLocalesFromNui);
            this._eventHandlerDictionary[$"__cfx_nui:{Events.Localization.GetAvailableLocalesFromNui}"] += new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
            {
                cb.Invoke(JsonConvert.SerializeObject(this._locales));
            });

            Cfx.API.RegisterNuiCallbackType(Events.Localization.ChangeLocalization);
            this._eventHandlerDictionary[$"__cfx_nui:{Events.Localization.ChangeLocalization}"] += new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
            {
                dynamic d = body;
                this._currentLocale = d.locale;
                this.LoadLocalization(this._currentLocale);
                cb.Invoke(JsonConvert.SerializeObject(this._currentLocalization));
            });
        }

        /// <inheritdoc />
        public void LoadAvailableLocales()
        {
            BaseScript.TriggerServerEvent(Events.Localization.GetAvailableLocalesFromServer, new Action<string>((locales) =>
            {
                foreach (var item in JArray.Parse(locales))
                {
                    this._locales.Add(new Locale(item["Key"].ToString(), item["Title"].ToString()));
                }
            }));
        }

        /// <inheritdoc />
        public void LoadDefaultLocalization()
        {
            this.LoadLocalization(this._defaultLocale);
        }

        /// <inheritdoc />
        public void LoadLocalization(string locale)
        {
            string data = Cfx.API.LoadResourceFile(Cfx.API.GetCurrentResourceName(),  $"/Localization/{locale}.json");
            if (string.IsNullOrEmpty(data))
            {
                Debug.WriteLine("Error while loading the localization.");
                data = "{}";
            }

            this._currentLocalization = JsonConvert.DeserializeObject<Localization>(data);
        }
    }
}