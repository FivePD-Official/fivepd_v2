using System.Collections.Generic;
using FivePD.Common.Attributes;
using FivePD.Common.ConfigModels;
using FivePD.Common.Exceptions;
using FivePD.Common.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using Serilog;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services;

/// <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.IConfigService" />
public class ConfigService : IConfigService
{
    private readonly IFileHandlerService _fileHandlerService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// </summary>
    /// <param name="fileHandlerService">The <see cref="IFileHandlerService"/> to use.</param>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public ConfigService(IFileHandlerService fileHandlerService, ILocalizationService localizationService, ILogger logger)
    {
        this._fileHandlerService = fileHandlerService;
        this._localizationService = localizationService;
        this._logger = logger;
    }

    /// <inheritdoc />
    [NoUsage]
    public DatabaseConfig GetDatabaseConfig()
    {
        return this.InternalLoadFile<DatabaseConfig>("/Config/database.json");
    }

    /// <inheritdoc />
    public List<Item> GetItems()
    {
        return this.InternalLoadFile<List<Item>>("/Config/items.json");
    }

    /// <inheritdoc />
    public MenuConfig GetMenuConfig()
    {
        return this.InternalLoadFile<MenuConfig>("/Config/menu.json");
    }

    /// <inheritdoc />
    public Vehicles GetVehiclesConfig()
    {
        return this.InternalLoadFile<Vehicles>("/Config/vehicles.json");
    }

    /// <inheritdoc />
    public List<Loadout> GetLoadoutsConfig()
    {
        return this.InternalLoadFile<List<Loadout>>("/Config/loadouts.json");
    }

    /// <inheritdoc />
    public FeaturesConfig GetFeaturesConfig()
    {
        return this.InternalLoadFile<FeaturesConfig>("/Config/features.json");
    }

    /// <inheritdoc />
    public List<string> GetFemaleFirstnames()
    {
        return this._fileHandlerService.LoadStringList("/Config/FemaleFirstnames.txt");
    }

    /// <inheritdoc />
    public List<string> GetMaleFirstnames()
    {
        return this._fileHandlerService.LoadStringList("/Config/MaleFirstnames.txt");
    }

    /// <inheritdoc />
    public List<string> GetLastnames()
    {
        return this._fileHandlerService.LoadStringList("/Config/Lastnames.txt");
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
            _ => "An error occured while parsing the {{path}} file, the default values will be used. Make sure that the JSON formatting is correct, we recommend using the following site to check it: https://jsonformatter.org/",
        };

        this._logger.Warning(exceptionMessage.ReplaceParams(
            new Dictionary<string, string>
            {
                { "path", $"\"{path}\"" },
            }));

        return file.Item1;
    }
}