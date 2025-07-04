using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Extensions;
using FivePD.Server.Attributes;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models.Requests;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.ILocalizationService" />
[Obfuscate]
public class LocalizationService : ILocalizationService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly INetworkEventService _networkEventService;
    private readonly IPlayerHandlerService _playerHandlerService;
    private readonly IConfigService _configService;
    private readonly ILogger _logger;

    private readonly Dictionary<string, string> _localizationCache = new();
    private readonly Dictionary<string, string> _availableLocalizations = new();

    /// <summary>Initializes a new instance of the <see cref="LocalizationService"/> class.</summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    /// <param name="playerHandlerService">The <see cref="IPlayerHandlerService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public LocalizationService(
        EventHandlerDictionary eventHandlerDictionary,
        INetworkEventService networkEventService,
        IPlayerHandlerService playerHandlerService,
        IConfigService configService,
        ILogger logger)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._networkEventService = networkEventService;
        this._playerHandlerService = playerHandlerService;
        this._configService = configService;
        this._logger = logger;
    }

    /// <inheritdoc />
    public ReadOnlyDictionary<string, string> AvailableLocalizations => new(this._availableLocalizations);

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this.LoadLocalizations();

        this._eventHandlerDictionary[Events.Localization.GetDefaultOrPreferred] += this.OnGetDefaultOrPreferred;
    }

    /// <inheritdoc />
    public string GetLocalization(string key)
    {
        return this._localizationCache.TryGetValue(key, out var localization) ? localization : null;
    }

    private void OnGetDefaultOrPreferred([FromSource] Player player, string guid)
    {
        string locale;

        var user = this._playerHandlerService.FindUserOfPlayer(player);
        if (user is null || string.IsNullOrEmpty(user.PreferredLanguage))
        {
            locale = this._configService.GeneralConfig.DefaultLanguage;
        }
        else
        {
            locale = user.PreferredLanguage;
        }

        this._networkEventService.TriggerEvent(player, guid, this.GetLocalization(locale));
    }

    [UIEvent(Events.Localization.GetAvailableLocalizations)]
    private void OnGetAvailableLocalizations([FromSource] Player player, string guid, string serializedRequest)
    {
        this._networkEventService.TriggerEventSerialized(player, guid, this._availableLocalizations);
    }

    private void LoadLocalizations()
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
                this._availableLocalizations[locale] = title;
                this._localizationCache[locale] = serializedLocalization;
            }
        }
        catch (Exception e)
        {
            this._logger.Warning("Could not load localizations: {Error}", e);
        }
    }
}