using System;
using System.Collections.Generic;
using System.Dynamic;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S1450

namespace FivePD.Gamemode.Client.Services;

/// <inheritdoc cref="FivePD.Gamemode.Client.Interfaces.ILocalizationService" />
internal class LocalizationService : ILocalizationService, IServiceWithEvents
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
    public void RegisterEvents()
    {
        this._eventHandlerDictionary.AddUIEvent(
            Events.Localization.GetLocalizationFromNui,
            new Action<ExpandoObject, CallbackDelegate>((_, cb) => { cb.Invoke(JsonConvert.SerializeObject(this.CurrentLocalization)); }));

        this._eventHandlerDictionary[Events.UI.Player.LocalizationChange] += new Action<string>(localization =>
        {
            this.CurrentLocalization = JsonConvert.DeserializeObject(localization);
            this._uiService.SendMessage(new JObject
            {
                { "type", Events.UI.Player.LocalizationChange },
                { "data", localization },
            });
        });

        this.LoadDefaultLocalization();
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
                    { "data", localization },
                });
            }));
    }

    /// <inheritdoc />
    public string Get(string key)
    {
        return this.CurrentLocalization.GetValueByPath(key);
    }

    private void LoadDefaultLocalization()
    {
        this.LoadLocalization("en");
    }
}