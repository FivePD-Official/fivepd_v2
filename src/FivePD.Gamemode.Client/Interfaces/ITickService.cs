using System;
using System.Threading.Tasks;

namespace FivePD.Gamemode.Client.Interfaces;

/// <summary>
/// A delegate used to add a handler for ticks.
/// </summary>
/// <param name="task">The function which will be called each tick.</param>
public delegate void AddTickHandler(Func<Task> task);

/// <summary>
/// A delegate used to remove a handler for ticks.
/// </summary>
/// <param name="task">The function to remove..</param>
public delegate void RemoveTickHandler(Func<Task> task);

/// <summary>
/// Provides an accessor for the resource's tick queue.
/// </summary>
public interface ITickService
{
    /// <summary>
    /// Provides a way to register and unregister ticks.
    /// </summary>
    public event Func<Task> OnTick;
}