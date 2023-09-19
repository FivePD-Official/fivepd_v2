using System;
using System.Collections.Generic;
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
}