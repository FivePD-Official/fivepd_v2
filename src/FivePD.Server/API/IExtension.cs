using System.Threading.Tasks;

namespace FivePD.Server.API;

/// <summary>
/// Represents an extension.
/// </summary>
public interface IExtension
{
    /// <summary>
    /// Executed when the addon is loaded by the GAR.
    /// </summary>
    /// <returns>
    /// A task which should complete when the addon is ready.
    /// </returns>
    Task OnStartedAsync();

    /// <summary>
    /// Executed when the GAR has requested the addon to stop.
    /// </summary>
    /// <returns>
    /// A task which should complete when the addon is ready.
    /// </returns>
    Task OnStoppedAsync();
}