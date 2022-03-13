// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Schemas;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc />
    public class LocalizationService : ILocalizationService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private List<Locale> _locales;

        /// <summary>Initializes a new instance of the <see cref="LocalizationService"/> class.</summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>>
        public LocalizationService(EventHandlerDictionary eventHandlerDictionary)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
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
                try
                {
                    string data = Cfx.API.LoadResourceFile(Cfx.API.GetCurrentResourceName(),  $"/Localization/{locale.Key}.json");

                    if (data is null)
                    {
                        Debug.WriteLine("Error while loading the localizations.");
                        data = "{}";
                    }

                    var localeData = JsonSchema.FromType<Localization>();
                    var validator = new JsonSchemaValidator();
                    var errors = validator.Validate(data, localeData);

                    if (errors.Count > 0)
                    {
                        foreach (var error in errors)
                        {
                            Debug.WriteLine("Localization error: " + error.Path + ": " + error.Kind);
                        }

                        // TODO: If there are any errors terminate FivePD.
                        return;
                    }

                    var localization = JsonConvert.DeserializeObject<Localization>(data);
                    locale.Title = localization?.Title ?? "???";
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }
    }
}