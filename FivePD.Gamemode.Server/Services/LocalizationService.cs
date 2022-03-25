// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.IO;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Localization;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc />
    public class LocalizationService : ILocalizationService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly IResourceService _resourceService;
        private List<Locale> _locales;

        /// <summary>Initializes a new instance of the <see cref="LocalizationService"/> class.</summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
        /// <param name="resourceService">The <see cref="ResourceService" /> to use.</param>
        public LocalizationService(EventHandlerDictionary eventHandlerDictionary, IResourceService resourceService)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._resourceService = resourceService;
        }

        /// <inheritdoc />
        public void RegisterEventHandlers()
        {
            this._eventHandlerDictionary[Events.Localization.GetAvailableLocalesFromServer] += new Action<NetworkCallbackDelegate>(cb =>
            {
                cb.Invoke(JsonConvert.SerializeObject(this._locales));
            });
        }

        /// <inheritdoc />
        public void LoadAvailableLocales()
        {
            try
            {
                this._locales = new List<Locale>();

                var path = AppDomain.CurrentDomain.BaseDirectory + "/Localization/";
                var files = Directory.GetFiles(path, "*.json");
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file).Split('.');
                    this._locales.Add(new Locale(fileName[0], string.Empty));
                }

                if (this._locales.Count == 0)
                {
                    this._locales.Add(new Locale("en", "English"));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading localization files: " + ex.Message);
            }
        }

        /// <inheritdoc />
        public void ValidateLocalizationFiles()
        {
            foreach (var locale in this._locales)
            {
                var localization = this._resourceService.LoadFile<Localization>($"/Localization/{locale.Key}.json");
                locale.Title = localization!.Title;
            }
        }
    }
}