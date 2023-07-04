using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#pragma warning disable SA1649

namespace FivePD.Server.QueryHelpers;

/// <summary>
/// Stores and processes your conditional sorts.
/// This sorting is mutually exclusive and the first sort satisfying the condition will be applied.
/// </summary>
/// <typeparam name="T">Type of Entity for which the sort is applicable.</typeparam>
public class Sorts<T>
{
    private readonly List<dynamic> _sorts;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sorts{T}"/> class.
    /// </summary>
    public Sorts()
    {
        this._sorts = new List<dynamic>();
    }

    /// <summary>
    /// Adds a conditional sort for your query.
    /// </summary>
    /// <typeparam name="TKey">The data type for the sort object.</typeparam>
    /// <param name="condition">The condition on which the the given sort will run.</param>
    /// <param name="expression">Sort expression.</param>
    /// <param name="byDescending">True if sorting is to be done be descending. By default data gets sorted by ascending.</param>
    /// <param name="priority">Determines the sequence of order by.</param>
    public void Add<TKey>(bool condition, Expression<Func<T, TKey>> expression, bool byDescending = false, int priority = 1)
    {
        this.Append(condition, expression, byDescending, priority);
    }

    /// <summary>
    /// Applies all sorts to the given query.
    /// </summary>
    /// <param name="query">The query to apply the sorts on.</param>
    /// <param name="sorts">The sorts to apply.</param>
    /// <returns>The <paramref name="query"/>.</returns>
    internal static IQueryable<T> ApplySorts(IQueryable<T> query, Sorts<T> sorts)
    {
        var isFirstSort = true;
        var validSortings = sorts.GetAll();
        IOrderedQueryable<T> orderedQuery = null;

        foreach (var sort in validSortings)
        {
            orderedQuery = isFirstSort
                ? ApplySorting(query, sort)
                : ApplySorting(orderedQuery, sort);
            isFirstSort = false;
        }

        return orderedQuery ?? query;
    }

    /// <summary>
    /// Returns a value indicating whether any of the sorts is applicable.
    /// </summary>
    /// <returns>A value indicating whether any of the sorts is applicable.</returns>
    internal bool IsValid()
    {
        return this._sorts.Any(s => s.Condition);
    }

    private static IOrderedQueryable<T> ApplySorting<TKey>(IQueryable<T> query, Sort<T, TKey> sort)
    {
        return sort.ByDescending
            ? query.OrderByDescending(sort.Expression)
            : query.OrderBy(sort.Expression);
    }

    private static IQueryable<T> ApplySorting<TKey>(IOrderedQueryable<T> query, Sort<T, TKey> sort)
    {
        return sort.ByDescending
            ? query.ThenByDescending(sort.Expression)
            : query.ThenBy(sort.Expression);
    }

    private void Append<TKey>(bool condition, Expression<Func<T, TKey>> expression, bool byDescending = false, int priority = 1)
    {
        this._sorts.Add(new Sort<T, TKey>
        {
            Condition = condition,
            Expression = expression,
            ByDescending = byDescending,
            Priority = priority,
        });
    }

    private dynamic GetAll()
    {
        return this._sorts.Where(s => s.Condition).OrderBy(s => s.Priority);
    }
}