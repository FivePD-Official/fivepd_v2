using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace FivePD.Server.QueryHelpers;

#pragma warning disable

/// <summary>
/// Stores and processes conditional filters for Dapper queries.
/// </summary>
public class DapperFilters
{
    private readonly List<DapperFilter> _filters;

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperFilters"/> class.
    /// </summary>
    public DapperFilters()
    {
        this._filters = new List<DapperFilter>();
    }

    /// <summary>
    /// Adds a conditional filter for your query.
    /// </summary>
    /// <param name="condition">The condition on which the filter will run.</param>
    /// <param name="sqlCondition">The SQL condition string (without WHERE).</param>
    /// <param name="parameters">The parameters for the SQL condition.</param>
    public void Add(bool condition, string sqlCondition, object parameters = null)
    {
        this._filters.Add(new DapperFilter
        {
            Condition = condition,
            SqlCondition = sqlCondition,
            Parameters = parameters,
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
    internal List<DapperFilter> Get()
    {
        return this._filters.Where(f => f.Condition).ToList();
    }

    /// <summary>
    /// Builds the WHERE clause for the SQL query and adds parameters to the DynamicParameters.
    /// </summary>
    /// <param name="parameters">The DynamicParameters to add filter parameters to.</param>
    /// <returns>The WHERE clause string (without "WHERE" keyword), or null if no filters are applicable.</returns>
    internal string BuildWhereClause(DynamicParameters parameters)
    {
        if (!IsValid())
        {
            return null;
        }

        var conditions = new List<string>();
        var filterList = Get();

        foreach (var filter in filterList)
        {
            if (!string.IsNullOrWhiteSpace(filter.SqlCondition))
            {
                conditions.Add(filter.SqlCondition);

                // Add parameters if provided
                if (filter.Parameters != null)
                {
                    var paramDict = filter.Parameters as IDictionary<string, object>;
                    if (paramDict != null)
                    {
                        foreach (var kvp in paramDict)
                        {
                            parameters.Add(kvp.Key, kvp.Value);
                        }
                    }
                    else
                    {
                        // If it's not a dictionary, try to add it as a single parameter object
                        // This is a simplified approach - in practice, you might need more sophisticated parameter handling
                        parameters.AddDynamicParams(filter.Parameters);
                    }
                }
            }
        }

        return conditions.Count > 0 ? string.Join(" AND ", conditions) : null;
    }
}