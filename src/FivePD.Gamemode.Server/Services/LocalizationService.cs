using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Utilities;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.ILocalizationService" />
    public class LocalizationService : ILocalizationService, IServiceWithEvents
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly IHttpService _httpService;
        private readonly Dictionary<string, string> _localizationCache = new Dictionary<string, string>();
        private object _defaultLocalization;
        private string _localizations;

        /// <summary>Initializes a new instance of the <see cref="LocalizationService"/> class.</summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
        /// <param name="httpService">The <see cref="IHttpService" /> to use.</param>
        public LocalizationService(EventHandlerDictionary eventHandlerDictionary, IHttpService httpService)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._httpService = httpService;
        }

        /// <inheritdoc/>
        public void RegisterEvents()
        {
            this._defaultLocalization = JsonConvert.DeserializeObject(this.GetLocalization("en"));
            this.GetLocalizations();

            this._eventHandlerDictionary[Events.Localization.GetAvailableLocalesFromServer] += new Action<NetworkCallbackDelegate>(cb =>
            {
                cb.Invoke(this._localizations);
            });
            this._eventHandlerDictionary[Events.Localization.GetLocalizationFromServer] += new Action<string, NetworkCallbackDelegate>((key, cb) =>
            {
                this._localizationCache.TryGetValue(key, out var localization);
                cb.Invoke(localization ?? this.GetLocalization(key));
            });
        }

        /// <inheritdoc />
        public string Get(string key)
        {
            return this._defaultLocalization.GetValueByPath(key);
        }

        private void GetLocalizations()
        {
            try
            {
                var result = this._httpService.Client.GetAsync("/localizations").Result;
                if (result.IsSuccessStatusCode)
                {
                    var content = result.Content.ReadAsStringAsync().Result;
                    this._localizations = content;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error loading localizations: " + e.Message);
            }
        }

        private string GetLocalization(string key)
        {
            try
            {
                var result = this._httpService.Client.GetAsync($"/localizations/{key}").Result;
                if (result.IsSuccessStatusCode)
                {
                    var content = result.Content.ReadAsStringAsync().Result;
                    this._localizationCache[key] = content;
                    return content;
                }

                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error loading localization ({key}): " + e.Message);
            }

            return null;
        }
    }
}