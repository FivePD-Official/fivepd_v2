using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace FivePD.Server.Database;

#pragma warning disable

public enum DbDialect
{
    MySql,
    PostgreSql,
    Sqlite
}

public class SqlBuilder
{
    private readonly DbDialect _dialect = DbDialect.MySql;
    private readonly List<string> _joins = new();
    private readonly List<string> _wheres = new();
    private readonly DynamicParameters _params = new();

    private string _groupBy;
    private string _selectOverride;
    private string _table;
    private string _orderBy;
    private int? _page;
    private const int PageSize = 10;

    public string Table => this._table;

    public SqlBuilder()
    {
    }

    public SqlBuilder(string table)
    {
        _table = table;
    }

    public SqlBuilder SetTable(string table)
    {
        this._table = table;
        return this;
    }

    public SqlBuilder If(bool condition, Action<SqlBuilder> action)
    {
        if (condition)
        {
            action(this);
        }

        return this;
    }

    public SqlBuilder Select(string select)
    {
        _selectOverride = select;
        return this;
    }

    public SqlBuilder GroupBy(string clause)
    {
        _groupBy = clause;
        return this;
    }

    public SqlBuilder InnerJoin(string table, string alias, string on)
    {
        _joins.Add($"INNER JOIN {table} {alias} ON {on}");
        return this;
    }

    public SqlBuilder LeftJoin(string table, string alias, string on)
    {
        _joins.Add($"LEFT JOIN {table} {alias} ON {on}");
        return this;
    }

    public SqlBuilder RightJoin(string table, string alias, string on)
    {
        _joins.Add($"RIGHT JOIN {table} {alias} ON {on}");
        return this;
    }

    public SqlBuilder Where(string condition, object param = null)
    {
        this.WhereInternal(condition, param);
        return this;
    }

    public SqlBuilder WhereIn<T>(string column, IEnumerable<T> values)
    {
        if (values is null || !values.Any())
        {
            return this;
        }

        var paramName = column.Replace(".", "_");
        _wheres.Add($"{column} IN @{paramName}");

        var dict = new Dictionary<string, object> { { paramName, values } };
        _params.AddDynamicParams(dict);

        return this;
    }

    public SqlBuilder WhereContains(string column, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return this;

        var paramName = column.Replace(".", "_");
        _wheres.Add($"{column} LIKE @{paramName}");

        var dict = new Dictionary<string, object> { { paramName, $"%{value}%" } };
        _params.AddDynamicParams(dict);

        return this;
    }

    public SqlBuilder OrderBy(string clause)
    {
        _orderBy = clause;
        return this;
    }

    public SqlBuilder ApplyPagination(int page)
    {
        _page = page;
        return this;
    }

    public (string sql, DynamicParameters parameters) Build(bool isCount = false)
    {
        var select = _selectOverride ?? (isCount ? "COUNT(*)" : $"{_table}.*");
        var sql = $"SELECT {select} FROM {_table} {string.Join(" ", _joins)}";

        if (_wheres.Any()) sql += $" WHERE {string.Join(" AND ", _wheres)}";

        if (!string.IsNullOrEmpty(_groupBy)) sql += $" GROUP BY {_groupBy}";

        if (!isCount)
        {
            if (!string.IsNullOrEmpty(_orderBy))
            {
                sql += $" ORDER BY {_orderBy}";
            }

            if (this._page.HasValue)
            {
                sql += FinalizePagination(this._page.Value);
            }
        }

        return (sql, _params);
    }

    private string FinalizePagination(int page)
    {
        var offset = (Math.Max(1, page) - 1) * PageSize;
        return _dialect switch
        {
            DbDialect.PostgreSql => $" LIMIT {PageSize} OFFSET {offset}",
            _ => $" LIMIT {offset}, {PageSize}" // MySQL / SQLite
        };
    }

    private void WhereInternal(string condition, object param = null)
    {
        _wheres.Add(condition);
        if (param is not null)
        {
            _params.AddDynamicParams(param);
        }
    }
}