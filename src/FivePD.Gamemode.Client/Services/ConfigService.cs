using System.Collections.Generic;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Common.ConfigModels;
using FivePD.Gamemode.Common.Exceptions;
using FivePD.Gamemode.Common.Extensions;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services;

/// <inheritdoc />
internal class ConfigService : IConfigService
{
    private readonly IFileHandlerService _fileHandlerService;
    private readonly ILoggerService _loggerService;
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// </summary>
    /// <param name="fileHandlerService">The <see cref="IFileHandlerService"/> to use.</param>
    /// <param name="loggerService">The <see cref="ILoggerService"/> to use.</param>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
    public ConfigService(IFileHandlerService fileHandlerService, ILoggerService loggerService, ILocalizationService localizationService)
    {
        this._fileHandlerService = fileHandlerService;
        this._loggerService = loggerService;
        this._localizationService = localizationService;
    }

    /// <inheritdoc/>
    public GeneralSettings GeneralSettings => this.InternalLoadFile<GeneralSettings>("/Config/general-settings.json");

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
    public MenuConfig GetMenuConfig()
    {
        return this.InternalLoadFile<MenuConfig>("/Config/menu.json");
    }

    private T InternalLoadFile<T>(string path, bool useCache = true)
    {
        var file = this._fileHandlerService.LoadFile<T>(path, useCache);

        if (file.Item2 is null)
        {
            return file.Item1;
        }

        var exceptionMessage = file.Item2 switch
        {
            EmptyConfigException _ => this._localizationService.Get("EXCEPTION.EMPTY_CONFIG"),
            _ => this._localizationService.Get("EXCEPTION.JSON_PARSE"),
        };

        this._loggerService.Warn(exceptionMessage.ReplaceParams(
            new Dictionary<string, string>
            {
                { "path", $"\"{path}\"" },
            }));

        return file.Item1;
    }
}