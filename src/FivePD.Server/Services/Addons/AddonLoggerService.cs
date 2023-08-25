using System;
using FivePD.Server.Interfaces.Addons;
using Serilog;

namespace FivePD.Server.Services.Addons;

/// <inheritdoc />
public class AddonLoggerService : IAddonLoggerService
{
    private readonly ILogger _logger;
    private readonly IAddonMetadataService _addonMetadataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddonLoggerService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="addonMetadataService"> The <see cref="IAddonMetadataService"/> to use.</param>
    public AddonLoggerService(ILogger logger, IAddonMetadataService addonMetadataService)
    {
        this._logger = logger;
        this._addonMetadataService = addonMetadataService;
    }

    /// <inheritdoc />
    public void Information(string message)
    {
        this.BuildLoggerWithContext().Information(message);
    }

    private ILogger BuildLoggerWithContext()
    {
        try
        {
            return this._logger.ForContext("SourceIdentifier", this._addonMetadataService.GetExecutingAddonDefinition().Name);
        }
        catch (Exception)
        {
            return this._logger.ForContext("SourceIdentifier", "GenericAddon");
        }
    }
}