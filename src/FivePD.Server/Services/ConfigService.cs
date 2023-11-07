using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.ConfigModels;
using FivePD.Common.Exceptions;
using FivePD.Common.Extensions;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IConfigService" />
[Obfuscate]
public class ConfigService : IConfigService, IOnInit
{
    private readonly IFileHandlerService _fileHandlerService;
    private readonly ILogger _logger;
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// </summary>
    /// <param name="fileHandlerService">The <see cref="IFileHandlerService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public ConfigService(IFileHandlerService fileHandlerService, ILogger logger, EventHandlerDictionary eventHandlerDictionary)
    {
        this._fileHandlerService = fileHandlerService;
        this._logger = logger;
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc />
    public List<Item> Items => this.InternalLoadFile<List<Item>>("/Config/items");

    /// <inheritdoc />
    public MenuConfig MenuConfig => this.InternalLoadFile<MenuConfig>("/Config/menu");

    /// <inheritdoc />
    public Vehicles VehiclesConfig => this.InternalLoadFile<Vehicles>("/Config/vehicles");

    /// <inheritdoc />
    public List<Loadout> LoadoutsConfig => this.InternalLoadFile<List<Loadout>>("/Config/loadouts");

    /// <inheritdoc />
    public GeneralConfig GeneralConfig => this.InternalLoadFile<GeneralConfig>("/Config/general");

    /// <inheritdoc />
    public List<string> FemaleFirstnames => this._fileHandlerService.LoadStringList("/Config/female-first-names.txt");

    /// <inheritdoc />
    public List<string> MaleFirstnames => this._fileHandlerService.LoadStringList("/Config/male-first-names.txt");

    /// <inheritdoc />
    public List<string> Lastnames => this._fileHandlerService.LoadStringList("/Config/last-names.txt");

    /// <inheritdoc />
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.Config.Get] += new Action<string, NetworkCallbackDelegate>(this.OnGetConfig);
    }

    private void OnGetConfig(string path, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var config = this.InternalLoadFile<object>(path);

        if (path == "/Config/general")
        {
            var generalConfig = (IInternalGeneralConfig)(GeneralConfig)config;
            generalConfig.Database = null;
        }

        networkCallbackDelegate.Invoke(JsonConvert.SerializeObject(config));
    }

    private T InternalLoadFile<T>(string path, bool useCache = true)
    {
        var file = this._fileHandlerService.LoadFile<T>(path, useCache);

        if (file.Item2 == null)
        {
            return file.Item1;
        }

        var exceptionMessage = file.Item2 switch
        {
            EmptyConfigException _ => "The {{path}} file is empty, the default values will be used.",
            _ => "An error occured while parsing the {{path}} file, the default values will be used. Make sure that the JSON or YAML formatting is correct.",
        };

        this._logger.Warning(exceptionMessage.ReplaceParams(
            new Dictionary<string, string>
            {
                { "path", $"\"{path}\"" },
            }));

        return file.Item1;
    }
}