using System;
using System.Collections.Generic;
using System.IO;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Extensions;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.ILocalizationService" />
[Obfuscate]
public class LocalizationService : ILocalizationService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly Dictionary<string, string> _localizationCache = new();

    /// <summary>Initializes a new instance of the <see cref="LocalizationService"/> class.</summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    public LocalizationService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc />
    public Dictionary<string, string> AvailableLocalizations { get; private set; } = new();

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this.GetLocalizations();

        this._eventHandlerDictionary[Events.Localization.GetLocalizationFromServer] += new Action<string, NetworkCallbackDelegate>((key, cb) =>
        {
            cb.Invoke(this.GetLocalization(key));
        });
    }

    /// <inheritdoc />
    public string GetLocalization(string key)
    {
        return this._localizationCache.TryGetValue(key, out var localization) ? localization : null;
    }

    #pragma warning disable
    private void GetLocalizations()
    {
        try
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localizations");
            var files = Directory.GetFiles(path, "*.json");
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var locale = fileName.Replace(".json", string.Empty); 
                var serializedLocalization = File.ReadAllText(Path.Combine(path, fileName));
                var deserializeLocalization = JsonConvert.DeserializeObject(serializedLocalization);
                var title = deserializeLocalization.GetValueByPath("TITLE");
                this.AvailableLocalizations[locale] = title;
                this._localizationCache[locale] = serializedLocalization;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Error loading localizations: " + e.Message);
        }
    }
}