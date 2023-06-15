using System.Collections.Generic;
using FivePD.Gamemode.Common.ConfigModels;
using FivePD.Gamemode.Common.Exceptions;
using FivePD.Gamemode.Common.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using Serilog;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services;

/// <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.IConfigService" />
public class ConfigService : IConfigService
{
    private readonly IFileHandlerService _fileHandlerService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// </summary>
    /// <param name="fileHandlerService">The <see cref="IFileHandlerService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public ConfigService(IFileHandlerService fileHandlerService, ILogger logger)
    {
        this._fileHandlerService = fileHandlerService;
        this._logger = logger;
    }

    /// <inheritdoc />
    public DatabaseConfig DatabaseConfig => this.InternalLoadFile<DatabaseConfig>("/Config/database.json");

    /// <inheritdoc />
    public List<Item> Items => this.InternalLoadFile<List<Item>>("/Config/items.json");

    /// <inheritdoc />
    public MenuConfig MenuConfig => this.InternalLoadFile<MenuConfig>("/Config/menu.json");

    /// <inheritdoc />
    public Vehicles VehiclesConfig => this.InternalLoadFile<Vehicles>("/Config/vehicles.json");

    /// <inheritdoc />
    public List<Loadout> LoadoutsConfig => this.InternalLoadFile<List<Loadout>>("/Config/loadouts.json");

    /// <inheritdoc />
    public FeaturesConfig FeaturesConfig => this.InternalLoadFile<FeaturesConfig>("/Config/features.json");

    /// <inheritdoc />
    public GeneralSettings GeneralSettings => this.InternalLoadFile<GeneralSettings>("/Config/general-settings.json");

    /// <inheritdoc />
    public List<string> FemaleFirstnames => this._fileHandlerService.LoadStringList("/Config/FemaleFirstnames.txt");

    /// <inheritdoc />
    public List<string> MaleFirstnames => this._fileHandlerService.LoadStringList("/Config/MaleFirstnames.txt");

    /// <inheritdoc />
    public List<string> Lastnames => this._fileHandlerService.LoadStringList("/Config/Lastnames.txt");

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