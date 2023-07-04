using System.Threading.Tasks;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Handles invocation on a tick by tick basis.
/// Useful for calling natives on a background thread when
/// we don't necessarily need an immediate result.
/// </summary>
internal interface IInteralTickQueueService
{
    /// <summary>
    /// Works through the queue, executing
    /// tasks and returning their values.
    /// </summary>
    /// <returns>A <see cref="Task" /> which completes when the tick is done being operated on.</returns>
    public Task OnTickAsync();
}