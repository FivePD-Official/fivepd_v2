using System.Threading.Tasks;
using CitizenFX.Core;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Handles interactions between entities.
/// </summary>
public interface IInteractionService
{
    /// <summary>
    /// Trys to locate a target entity and alert them to stop.
    /// </summary>
    /// <param name="player">The player who invoked this action.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task TryAlertEntityAsync(Player player);
}