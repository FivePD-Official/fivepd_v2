using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Utilities;
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
        private readonly IUIService _uiService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
        /// <param name="uiService">The <see cref="IUIService" /> to use.</param>
        public LocalizationService(EventHandlerDictionary eventHandlerDictionary, IUIService uiService)
        {
            this.Locales = new Dictionary<string, string>();
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._uiService = uiService;
        }

        /// <inheritdoc />
        public event EventHandler<string> LocalizationChanged;

        /// <inheritdoc />
        public Dictionary<string, string> Locales { get; private set; }

        /// <inheritdoc />
        public object CurrentLocalization { get; private set; }

        /// <inheritdoc />
        public void RegisterEventHandlers()
        {
            this._eventHandlerDictionary.AddUIEvent(
                Events.Localization.GetLocalizationFromNui,
                new Action<ExpandoObject, CallbackDelegate>((body, cb) => { cb.Invoke(JsonConvert.SerializeObject(this.CurrentLocalization)); }));
        }

        /// <inheritdoc />
        public void LoadDefaultLocalization()
        {
            this.LoadLocalization("en");
        }

        /// <inheritdoc />
        public void LoadLocalization(string locale)
        {
            BaseScript.TriggerServerEvent(
                Events.Localization.GetLocalizationFromServer,
                locale,
                new Action<string>(localization =>
                {
                    this.CurrentLocalization = JsonConvert.DeserializeObject(localization);
                    this.LocalizationChanged?.Invoke(this, locale);
                    this._uiService.SendMessage(new JObject
                    {
                        { "type", Events.UI.Player.LocalizationChange },
                        { "data", JObject.FromObject(this.CurrentLocalization) },
                    });
                }));
        }

        /// <inheritdoc />
        public string Get(string key)
        {
            return this.CurrentLocalization.GetValueByPath(key);
        }
    }
}