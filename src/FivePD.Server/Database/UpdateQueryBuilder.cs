using System.Collections.Generic;
using Dapper;

namespace FivePD.Server.Database;

/// <summary>
/// Responsible for constructing and building SQL update queries with support for conditional column updates.
/// Provides a chainable API for adding columns and values to an update query.
/// </summary>
public class UpdateQueryBuilder
{
    private readonly List<string> _columns = new();
    private readonly DynamicParameters _params = new();
    private readonly ChangeTracker _changeTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateQueryBuilder"/> class.
    /// </summary>
    public UpdateQueryBuilder()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateQueryBuilder"/> class with an optional change tracker.
    /// </summary>
    /// <param name="changeTracker">The change tracker to automatically check for property modifications.</param>
    public UpdateQueryBuilder(ChangeTracker changeTracker)
    {
        this._changeTracker = changeTracker;
    }

    /// <summary>
    /// Adds a column and its corresponding value to the update query if the specified condition is met.
    /// </summary>
    /// <typeparam name="T">The type of the value to be added to the query.</typeparam>
    /// <param name="columnName">The name of the column to be updated.</param>
    /// <param name="value">The value to assign to the specified column.</param>
    /// <param name="condition">A boolean condition that determines whether the column and value are added to the query.</param>
    /// <returns>An instance of <see cref="UpdateQueryBuilder"/> to allow method chaining.</returns>
    public UpdateQueryBuilder Add<T>(string columnName, T value, bool condition)
    {
        if (!condition)
        {
            return this;
        }

        this._columns.Add($"{columnName} = @{columnName}");
        this._params.Add(columnName, value);

        return this;
    }

    /// <summary>
    /// Adds a column and its corresponding value to the update query if the property was changed.
    /// Uses the ChangeTracker provided in the constructor to automatically check for modifications.
    /// </summary>
    /// <typeparam name="T">The type of the value to be added to the query.</typeparam>
    /// <param name="columnName">The name of the column to be updated.</param>
    /// <param name="value">The value to assign to the specified column.</param>
    /// <returns>An instance of <see cref="UpdateQueryBuilder"/> to allow method chaining.</returns>
    public UpdateQueryBuilder Add<T>(string columnName, T value)
    {
        var shouldAdd = this._changeTracker is null || this._changeTracker.IsChanged(columnName);
        return this.Add(columnName, value, shouldAdd);
    }

    /// <summary>
    /// Constructs and finalizes an SQL update query based on the provided table name, identifier column, and identifier value.
    /// </summary>
    /// <param name="table">The name of the table to be updated.</param>
    /// <param name="idColumn">The name of the column that identifies the row to be updated.</param>
    /// <param name="idValue">The value of the identifier column that determines the specific row to update.</param>
    /// <returns>A tuple containing the constructed SQL query as a string and the associated dynamic parameters for the query.</returns>
    public (string Sql, DynamicParameters Params) Build(string table, string idColumn, object idValue)
    {
        this._params.Add(idColumn, idValue);
        var sql = $"UPDATE {table} SET {string.Join(", ", this._columns)} WHERE {idColumn} = @{idColumn}";
        return (sql, this._params);
    }
}