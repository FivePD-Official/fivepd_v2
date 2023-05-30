using System;
using System.Threading.Tasks;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Handles invocation on a tick by tick basis.
/// Useful for calling natives on a background thread when
/// we don't necessarily need an immediate result.
/// </summary>
public interface ITickQueueService
{
    /// <summary>
    /// Works through the queue, executing
    /// tasks and returning their values.
    /// </summary>
    /// <returns>A <see cref="Task" /> which completes when the tick is done being operated on.</returns>
    public Task OnTickAsync();

    /// <summary>
    /// Pushes an action to the tick queue,
    /// and requests that the system runs a tick
    /// for the current resource ASAP.
    /// </summary>
    /// <param name="action">An action to execute and complete upon execution.</param>
    /// <returns>A task which completes upon execution.</returns>
    public Task EnqueueAsync(Action action);

    /// <summary>
    /// See <see cref="EnqueueAsync"/>.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="action">An action to execute and complete upon execution.</param>
    /// <returns>A task which completes upon execution.</returns>
    public Task<T> EnqueueAsync<T>(Func<T> action);

    /// <summary>
    /// Pushes an action to the tick queue which will be invoked at the given interval.
    /// </summary>
    /// <param name="action">An action to execute and complete upon execution.</param>
    /// <param name="intervalInMs">The interval to run the action in milliseconds.</param>
    /// <returns>The enqueued task's ID.</returns>
    public Guid EnqueueInterval(Action action, int intervalInMs);

    /// <summary>
    /// Dequeues an interval task.
    /// </summary>
    /// <param name="guid">The ID of the task.</param>
    public void DequeueInterval(Guid guid);
}