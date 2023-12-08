using System;
using System.Collections.Generic;
using System.Dynamic;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S1450

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.ILocalizationService" />
internal class LocalizationService : ILocalizationService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    public LocalizationService(EventHandlerDictionary eventHandlerDictionary)
    {
        this.Locales = new Dictionary<string, string>();
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc />
    public Dictionary<string, string> Locales { get; private set; }

    /// <inheritdoc />
    public object CurrentLocalization { get; private set; }

    /// <inheritdoc />
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Localization.GetAvailableLocalizations);
        this._eventHandlerDictionary.AddUIEvent(
            Events.Localization.GetLocalizationFromNui,
            new Action<ExpandoObject, CallbackDelegate>((_, cb) => { cb.Invoke(JsonConvert.SerializeObject(this.CurrentLocalization)); }));

        this._eventHandlerDictionary[Events.UI.Player.LocalizationChange] += new Action<string>(localization =>
        {
            this.CurrentLocalization = JsonConvert.DeserializeObject(localization);
            Cfx.API.SendNuiMessage(JsonConvert.SerializeObject(new JObject
            {
                { "Event", Events.UI.Player.LocalizationChange },
                { "Data", localization },
            }));
        });

        this.LoadLocalization();
    }

    /// <inheritdoc />
    public string Get(string key)
    {
        return this.CurrentLocalization.GetValueByPath(key);
    }

    private void LoadLocalization()
    {
        BaseScript.TriggerServerEvent(
            Events.Localization.GetDefaultOrPreferred,
            new Action<string, string>((locale, localization) =>
            {
                this.CurrentLocalization = JsonConvert.DeserializeObject(localization);
                Cfx.API.SendNuiMessage(JsonConvert.SerializeObject(new JObject
                {
                    { "Event", Events.UI.Player.LocalizationChange },
                    { "Data", localization },
                }));
            }));
    }
}