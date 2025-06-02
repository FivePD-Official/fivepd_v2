using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FivePD.Server.Utilities;

/// <summary>
/// A collection of helper methods for tasks.
/// </summary>
public static class TaskHelpers
{
    /// <summary>
    /// Runs a collection of tasks sequentially.
    /// </summary>
    /// <param name="operations">The operations to run.</param>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <returns>The results of the operations.</returns>
    public static async Task<IEnumerable<T>> RunSequentiallyAsync<T>(IEnumerable<Func<Task<T>>> operations)
    {
        var results = new List<T>();
        foreach (var op in operations)
        {
            results.Add(await op());
        }

        return results;
    }

    /// <summary>
    /// Delays execution until a specified condition is met, the maximum delay time elapses, or an external cancellation is requested.
    /// </summary>
    /// <param name="condition">A function that returns a boolean value indicating the condition to be checked.</param>
    /// <param name="maxDelay">The maximum amount of time to wait for the condition to be met.</param>
    /// <param name="checkInterval">The interval at which the condition is checked. If not specified, defaults to 1 second.</param>
    /// <param name="cancellationTokenSource">An optional <see cref="CancellationTokenSource"/> that can be used to externally cancel the wait. If the token is canceled, the method will return false.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is true if the condition is met within the
    /// specified maximum delay; otherwise, false if the maximum delay is reached or if the operation is canceled.
    /// </returns>
    public static async Task<bool> DelayUntilAsync(
        Func<bool> condition,
        TimeSpan maxDelay,
        TimeSpan checkInterval = default,
        CancellationTokenSource cancellationTokenSource = null)
    {
        checkInterval = checkInterval == default ? TimeSpan.FromSeconds(1) : checkInterval;
        using var cts = new CancellationTokenSource(maxDelay);
        while (true)
        {
            if (cancellationTokenSource is not null && cancellationTokenSource.IsCancellationRequested)
            {
                return false;
            }

            var result = condition();
            if (result)
            {
                return true;
            }

            if (cts.Token.IsCancellationRequested)
            {
                return false;
            }

            try
            {
                await Task.Delay(checkInterval, cts.Token);
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }
    }
}