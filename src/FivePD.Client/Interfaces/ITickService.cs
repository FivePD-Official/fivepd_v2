using System;
using System.Threading.Tasks;

namespace FivePD.Client.Interfaces;

/// <summary>
/// A delegate used to add a handler for ticks.
/// </summary>
/// <param name="task">The function which will be called each tick.</param>
internal delegate void AddTickHandler(Func<Task> task);

/// <summary>
/// A delegate used to remove a handler for ticks.
/// </summary>
/// <param name="task">The function to remove..</param>
internal delegate void RemoveTickHandler(Func<Task> task);

/// <summary>
/// Provides an accessor for the resource's tick queue.
/// </summary>
internal interface ITickService
{
    /// <summary>
    /// Provides a way to register and unregister ticks.
    /// </summary>
    public event Func<Task> OnTick;

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