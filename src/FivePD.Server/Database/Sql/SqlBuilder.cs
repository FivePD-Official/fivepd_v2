using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CitizenFX.Core;
using Dapper;

namespace FivePD.Server.Database.Sql;

#pragma warning disable

public class SqlBuilder : IQueryBuilder, ISelectBuilder, IDeleteBuilder
{
    private readonly DbType _dialect = DbType.MySql;
    private readonly List<JoinMetadata> _joinMetadata = new();
    private readonly List<string> _wheres = new();
    private readonly DynamicParameters _params = new();

    private Type _baseType;
    private bool _isDelete;
    private string _selectOverride;
    private string _groupBy;
    private string _from;
    private string _orderBy;
    private int? _page;
    private const int PageSize = 10;

    private SqlBuilder(string table)
    {
        this._from = table;
    }

    public static IQueryBuilder From(string table)
    {
        return new SqlBuilder(table);
    }

    IQueryBuilder IFilterable<IQueryBuilder>.If(bool condition, Action<IQueryBuilder> action) => InternalIf(condition, action);

    ISelectBuilder IFilterable<ISelectBuilder>.If(bool condition, Action<ISelectBuilder> action) => InternalIf(condition, action);

    IDeleteBuilder IFilterable<IDeleteBuilder>.If(bool condition, Action<IDeleteBuilder> action) => InternalIf(condition, action);

    SqlBuilderResult IQueryBuilder.Build() => InternalBuild();
    SqlBuilderResult ISelectBuilder.Build() => InternalBuild();
    SqlBuilderResult IDeleteBuilder.Build() => InternalBuild();
    SqlBuilderResult IQueryBuilder.BuildCount() => InternalBuildCount();
    SqlBuilderResult ISelectBuilder.BuildCount() => InternalBuildCount();
    SqlBuilderResult IDeleteBuilder.BuildCount() => InternalBuildCount();

    public ISelectBuilder Select(string select, Type type = null)
    {
        _selectOverride = select;
        this._baseType = type;
        return this;
    }

    public ISelectBuilder GroupBy(string clause)
    {
        _groupBy = clause;
        return this;
    }

    public ISelectBuilder OrderBy(string clause)
    {
        _orderBy = clause;
        return this;
    }

    public SqlBuilder ApplyPagination(int page)
    {
        _page = page;
        return this;
    }

    public IDeleteBuilder Delete()
    {
        _isDelete = true;
        return this;
    }

    public IQueryBuilder InnerJoin(string table, string alias, string on, Type type = null, string splitColumn = "Id")
    {
        var sql = $"INNER JOIN {table} {alias} ON {on}";
        _joinMetadata.Add(new JoinMetadata(sql, type, splitColumn));
        return this;
    }

    public IQueryBuilder LeftJoin(string table, string alias, string on, Type type = null, string splitColumn = "Id")
    {
        var sql = $"LEFT JOIN {table} {alias} ON {on}";
        _joinMetadata.Add(new JoinMetadata(sql, type, splitColumn));
        return this;
    }

    public IQueryBuilder RightJoin(string table, string alias, string on, Type type = null, string splitColumn = "Id")
    {
        var sql = $"RIGHT JOIN {table} {alias} ON {on}";
        _joinMetadata.Add(new JoinMetadata(sql, type, splitColumn));
        return this;
    }

    public IQueryBuilder Where(string condition, object param = null)
    {
        this.InternalWhere(condition, param);
        return this;
    }

    public IQueryBuilder WhereAnyLicenseMatches(Player player)
    {
        if (player is null)
        {
            return this;
        }

        this.InternalWhere(
            "IdentifierLicense = @License OR IdentifierDiscord = @Discord OR IdentifierSteam = @Steam OR IdentifierXbl = @Xbl OR IdentifierLive = @Live",
            new
            {
                License = player.Identifiers["license"] ?? string.Empty,
                Discord = player.Identifiers["discord"] ?? string.Empty,
                Steam = player.Identifiers["steam"] ?? string.Empty,
                Xbl = player.Identifiers["xbl"] ?? string.Empty,
                Live = player.Identifiers["live"] ?? string.Empty,
            });
        return this;
    }

    public IQueryBuilder WhereIn<T>(string column, IEnumerable<T> values)
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

    public IQueryBuilder WhereNotIn<T>(string column, IEnumerable<T> values)
    {
        if (values is null || !values.Any())
        {
            return this;
        }

        var paramName = "not_" + column.Replace(".", "_");
        _wheres.Add($"{column} NOT IN @{paramName}");
        _params.Add(paramName, values);

        return this;
    }

    public IQueryBuilder WhereContains(string column, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return this;

        var paramName = column.Replace(".", "_");
        _wheres.Add($"{column} LIKE @{paramName}");

        var dict = new Dictionary<string, object> { { paramName, $"%{value}%" } };
        _params.AddDynamicParams(dict);

        return this;
    }

    private SqlBuilderResult InternalBuild()
    {
        var sb = new StringBuilder();
        if (_isDelete)
        {
            sb.Append($"DELETE FROM {this._from}");
        }
        else
        {
            var select = _selectOverride ?? $"{this._from}.*";
            sb.Append($"SELECT {select} FROM {this._from} {string.Join(" ", this._joinMetadata.Select(x => x.JoinSql))}");
        }

        if (_wheres.Any())
        {
            sb.Append($" WHERE {string.Join(" AND ", _wheres)}");
        }

        if (!string.IsNullOrEmpty(_groupBy))
        {
            sb.Append($" GROUP BY {_groupBy}");
        }

        if (!this._isDelete)
        {
            if (!string.IsNullOrEmpty(_orderBy))
            {
                sb.Append($" ORDER BY {_orderBy}");
            }

            if (this._page.HasValue)
            {
                var offset = (Math.Max(1, this._page.Value) - 1) * PageSize;
                sb.Append(_dialect switch
                {
                    DbType.PostgreSql => $" LIMIT {PageSize} OFFSET {offset}",
                    _ => $" LIMIT {offset}, {PageSize}" // MySQL / SQLite
                });
            }
        }

        var sql = sb.ToString();
        var joinMetadata = this._joinMetadata.ToList();
        var splitColumns = joinMetadata.Select(x => x.SplitColumn).ToList();
        var splitOn = splitColumns.Count > 0 ? string.Join(",", splitColumns) : "Id";

        var allTypes = new List<Type> { this._baseType };
        allTypes.AddRange(_joinMetadata.Select(x => x.Type));

        return new SqlBuilderResult(sql, _params, allTypes.ToArray(), splitOn);
    }

    private SqlBuilderResult InternalBuildCount()
    {
        var sb = new StringBuilder();
        sb.Append($"SELECT 1 FROM {_from} {string.Join(" ", _joinMetadata.Select(x => x.JoinSql))}");

        if (_wheres.Any())
        {
            sb.Append($" WHERE {string.Join(" AND ", _wheres)}");
        }

        if (!string.IsNullOrEmpty(_groupBy))
        {
            var subquerySql = $"{sb} GROUP BY {_groupBy}";
            var finalSql = $"SELECT COUNT(*) FROM ({subquerySql}) AS countTable";

            return new SqlBuilderResult(finalSql, _params, null, null);
        }

        var countSql = sb.Replace("SELECT 1", "SELECT COUNT(*)").ToString();
        return new SqlBuilderResult(countSql, _params, null, null);
    }

    private void InternalWhere(string condition, object param = null)
    {
        _wheres.Add(condition);
        if (param is not null)
        {
            _params.AddDynamicParams(param);
        }
    }

    private T InternalIf<T>(bool condition, Action<T> action) where T : class
    {
        if (condition)
        {
            action(this as T);
        }

        return this as T;
    }
}