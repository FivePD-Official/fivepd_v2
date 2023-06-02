using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#pragma warning disable SA1649

namespace FivePD.Gamemode.Server.QueryHelpers;

/// <summary>
/// Stores and processes your conditional filters.
/// </summary>
/// <typeparam name="T">Type of Entity for which the filters are applicable.</typeparam>
public class Filters<T>
{
    private readonly List<Filter<T>> _filters;

    /// <summary>
    /// Initializes a new instance of the <see cref="Filters{T}"/> class.
    /// </summary>
    public Filters()
    {
        this._filters = new List<Filter<T>>();
    }

    /// <summary>
    /// Adds a conditional filter for your query.
    /// </summary>
    /// <param name="condition">The condition on which the the given filter will run.</param>
    /// <param name="expression">Filter expression.</param>
    public void Add(bool condition, Expression<Func<T, bool>> expression)
    {
        this._filters.Add(new Filter<T>
        {
            Condition = condition,
            Expression = expression,
        });
    }

    /// <summary>
    /// Returns a value indicating whether any of the filters is applicable.
    /// </summary>
    /// <returns>A value indicating whether any of the filters is applicable.</returns>
    internal bool IsValid()
    {
        return this._filters.Any(f => f.Condition);
    }

    /// <summary>
    /// Returns all filters which had been added to this instance.
    /// </summary>
    /// <returns>All filters.</returns>
    internal List<Filter<T>> Get()
    {
        return this._filters.Where(f => f.Condition).ToList();
    }
}