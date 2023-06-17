using System.Threading.Tasks;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Common.ConfigModels;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services;

/// <inheritdoc />
internal class ConfigService : IConfigService
{
    private readonly IFileHandlerService _fileHandlerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// </summary>
    /// <param name="fileHandlerService">The <see cref="IFileHandlerService"/> to use.</param>
    public ConfigService(IFileHandlerService fileHandlerService)
    {
        this._fileHandlerService = fileHandlerService;
    }

    /// <inheritdoc/>
    public Task<GeneralConfig> GeneralConfig => this.InternalLoadFileAsync<GeneralConfig>("/Config/general");

    private Task<T> InternalLoadFileAsync<T>(string path)
    {
        return this._fileHandlerService.LoadFileAsync<T>(path);
    }
}