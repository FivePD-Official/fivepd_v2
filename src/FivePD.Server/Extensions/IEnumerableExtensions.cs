using System;
using System.Collections.Generic;
using System.Linq;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Extensions;

/// <summary>
/// Provides extension methods for generating random items from collections.
/// </summary>
public static class IEnumerableExtensions
{
    private static readonly IRandomService RandomService = ServiceLocator.GetService<IRandomService>();

    /// <summary>
    /// Returns either all items or a random subset of items from the collection.
    /// </summary>
    /// <typeparam name="TItem">The type of items in the collection.</typeparam>
    /// <param name="items">The collection of possible items.</param>
    /// <param name="includeAll">Whether to include all items or a random subset.</param>
    /// <param name="maxCount">Maximum number of items to include when not including all.</param>
    /// <returns>A filtered collection of items. Returns all items if includeAll is true, otherwise returns a random subset of 0 to maxCount items.</returns>
    public static IEnumerable<TItem> TakeRandomSubset<TItem>(this IEnumerable<TItem> items, bool includeAll = false, int maxCount = 6)
        where TItem : class
    {
        if (includeAll)
        {
            return items;
        }

        return items
            .OrderBy(_ => Guid.NewGuid())
            .Take(RandomService.Next(0, maxCount + 1));
    }
}