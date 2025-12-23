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
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// </summary>
    /// <param name="fileHandlerService">The <see cref="IFileHandlerService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="networkEventService">The <see cref="NetworkEventService"/> to use.</param>
    public ConfigService(IFileHandlerService fileHandlerService, ILogger logger, EventHandlerDictionary eventHandlerDictionary, INetworkEventService networkEventService)
    {
        this._fileHandlerService = fileHandlerService;
        this._logger = logger;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._networkEventService = networkEventService;
    }

    /// <inheritdoc />
    public List<Item> Items => this.InternalLoadFile<List<Item>>("/Config/items");

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
        this._eventHandlerDictionary[Events.Config.Get] += new Action<Player, string, string>(this.OnGetConfig);
    }

    private void OnGetConfig([FromSource] Player player, string guid, string path)
    {
        if (path == "/Config/general")
        {
            var config = this.InternalLoadFile<GeneralConfig>(path);
            var clientConfig = config with { Database = new() };
            this._networkEventService.TriggerEvent(player, guid, JsonConvert.SerializeObject(clientConfig));
            return;
        }

        var otherConfig = this.InternalLoadFile<object>(path);
        this._networkEventService.TriggerEvent(player, guid, JsonConvert.SerializeObject(otherConfig));
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

        this._logger.Warning(exceptionMessage.Replace("{{path}}", path));

        return file.Item1;
    }
}