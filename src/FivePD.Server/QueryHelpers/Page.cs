using System.Collections.Generic;

#pragma warning disable SA1649

namespace FivePD.Server.QueryHelpers;

/// <summary>
/// Stores the filtered, sorted and paginated data for the given entity type.
/// </summary>
/// <typeparam name="T">Type of Entity for which pagination is being implemented.</typeparam>
public class Page<T>
{
    /// <summary>
    /// Gets or sets IEnumerable of the final sorted, filtered and paginated data.
    /// </summary>
    public IEnumerable<T> Items { get; set; }

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    /// Gets or sets the number of entities in a page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of entities that exist for the query.
    /// </summary>
    public int ItemCount { get; set; }
}