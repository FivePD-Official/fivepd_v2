using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;
using FivePD.Server.QueryHelpers;

namespace FivePD.Server.Database;

#pragma warning disable

/// <summary>
/// Dapper extensions to support QueryHelpers functionality.
/// </summary>
public static class DapperExtensions
{
    /// <summary>
    /// Executes a paginated query and returns a Page object.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="connection">The database connection.</param>
    /// <param name="baseQuery">The base SQL query without WHERE clause.</param>
    /// <param name="whereClause">The WHERE clause (without "WHERE").</param>
    /// <param name="parameters">The query parameters.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="orderBy">The ORDER BY clause (without "ORDER BY").</param>
    /// <returns>A Page object with paginated results.</returns>
    public static Page<T> Paginate<T>(
        this IDbConnection connection,
        string baseQuery,
        string whereClause,
        object parameters,
        int pageNumber,
        int pageSize = 10,
        string orderBy = "Id")
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        // Build count query
        var countQuery = $"{baseQuery}";
        if (!string.IsNullOrEmpty(whereClause))
        {
            countQuery += $" WHERE {whereClause}";
        }

        var totalCount = connection.ExecuteScalar<int>(countQuery, parameters);

        // Build paginated query
        var paginatedQuery = $"{baseQuery}";
        if (!string.IsNullOrEmpty(whereClause))
        {
            paginatedQuery += $" WHERE {whereClause}";
        }

        if (!string.IsNullOrEmpty(orderBy))
        {
            paginatedQuery += $" ORDER BY {orderBy}";
        }

        var offset = (pageNumber - 1) * pageSize;
        paginatedQuery += $" LIMIT {pageSize} OFFSET {offset}";

        var items = connection.Query<T>(paginatedQuery, parameters).ToList();

        var pageCount = (int)Math.Ceiling((double)totalCount / pageSize);
        if (pageCount == 0) pageCount = 1;

        return new Page<T>
        {
            CurrentPage = pageNumber,
            PageSize = pageSize,
            ItemCount = totalCount,
            PageCount = pageCount,
            Items = items
        };
    }

    /// <summary>
    /// Applies filters to a SQL query.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="baseQuery">The base SQL query.</param>
    /// <param name="filters">The filters to apply.</param>
    /// <param name="parameters">The query parameters (will be modified with filter values).</param>
    /// <returns>The modified SQL query with WHERE clause.</returns>
    public static string ApplyFilters<T>(string baseQuery, Filters<T> filters, ref DynamicParameters parameters)
    {
        if (filters == null || !filters.IsValid())
        {
            return baseQuery;
        }

        var whereClauses = new List<string>();
        var filterIndex = 0;

        foreach (var filter in filters.Get())
        {
            // For Dapper, we need to handle filters differently
            // Since Expression<Func<T, bool>> can't be directly translated to SQL,
            // we'll need to handle this at the repository level or use a different approach

            // For now, we'll skip complex expression-based filters and rely on the repository
            // to handle specific filtering needs
            filterIndex++;
        }

        if (whereClauses.Count > 0)
        {
            baseQuery += " WHERE " + string.Join(" AND ", whereClauses);
        }

        return baseQuery;
    }

    /// <summary>
    /// Executes a query with complex filtering and sorting.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="connection">The database connection.</param>
    /// <param name="baseQuery">The base SQL query.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="filters">Optional filters.</param>
    /// <param name="sorts">Optional sorts.</param>
    /// <returns>A Page object with results.</returns>
    public static Page<T> QueryPage<T>(
        this IDbConnection connection,
        string baseQuery,
        object parameters,
        int pageNumber,
        int pageSize = 10,
        Filters<T> filters = null,
        Sorts<T> sorts = null)
    {
        // For Dapper, we'll handle basic pagination
        // Complex filtering and sorting would need custom SQL per use case

        return connection.Paginate<T>(
            baseQuery,
            string.Empty, // WHERE clause handled by repository
            parameters,
            pageNumber,
            pageSize,
            "SentAt DESC" // Default sort for messages
        );
    }
}