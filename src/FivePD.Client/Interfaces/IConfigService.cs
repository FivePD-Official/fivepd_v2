using System.Threading.Tasks;
using FivePD.Common.ConfigModels;

namespace FivePD.Client.Interfaces;

/// <summary>
/// Provides utility methods for the FivePD resource itself.
/// </summary>
internal interface IConfigService
{
    /// <summary>
    /// Gets the content of the "Config/general-settings.json" file.
    /// </summary>
    public Task<GeneralConfig> GeneralConfig { get; }
}