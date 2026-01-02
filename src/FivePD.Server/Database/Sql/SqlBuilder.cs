using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CitizenFX.Core;
using Dapper;
using FivePD.Server.Database.Sql.Dialect;

namespace FivePD.Server.Database.Sql;

#pragma warning disable

public class SqlBuilder : IQueryBuilder, ISelectBuilder, IInsertBuilder, IInsertTableBuilder, IDeleteBuilder, IUpdateBuilder
{
    private readonly ISqlDialect _dialect = SqlDialectRegistry.Current;
    private readonly List<JoinMetadata> _joinMetadata = new();
    private readonly List<string> _selects = new();
    private readonly List<Type> _types = new();
    private readonly List<string> _wheres = new();
    private readonly List<string> _insertColumns = new();
    private readonly DynamicParameters _params = new();
    private readonly List<string> _keyColumns = new();
    private readonly List<string> _updates = new();

    private bool _isUpdate;
    private bool _isInsert;
    private bool _isUpsert;
    private bool _isDelete;
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

    public static IUpdateBuilder Update(string table)
    {
        return new SqlBuilder(table)
        {
            _isUpdate = true
        };
    }

    public IUpdateBuilder Set(string column, object value)
    {
        var paramName = "set_" + column.Replace(".", "_");
        _updates.Add($"{column} = @{paramName}");
        _params.Add(paramName, value);
        return this;
    }

    public IUpdateBuilder SetTracked<T>(T entity, string idColumn = null, object id = null)
        where T : ChangeTracker
    {
        if (entity is null)
        {
            return this;
        }

        var properties = entity.GetType().GetProperties();
        foreach (var prop in properties)
        {
            if (!entity.IsChanged(prop.Name))
            {
                continue;
            }

            var value = prop.GetValue(entity);
            this.Set(prop.Name, value);
        }

        if (!string.IsNullOrEmpty(idColumn) && id is not null)
        {
            this.WhereEquals(idColumn, id);
        }

        return this;
    }

    public static IInsertTableBuilder Into(string table)
    {
        return new SqlBuilder(table);
    }

    public IInsertBuilder Insert(object entity)
    {
        _isInsert = true;
        _params.AddDynamicParams(entity);

        if (entity is not null)
        {
            var properties = entity.GetType().GetProperties();
            foreach (var prop in properties)
            {
                _insertColumns.Add(prop.Name);
            }
        }

        return this;
    }

    public IInsertBuilder OnDuplicateKeyUpdate(params string[] customKeys)
    {
        this._isUpsert = true;
        if (customKeys.Length > 0)
        {
            _keyColumns.Clear();
            _keyColumns.AddRange(customKeys);
        }

        return this;
    }

    IQueryBuilder IFilterable<IQueryBuilder>.If(bool condition, Action<IQueryBuilder> action) => this.InternalIf(condition, action);

    ISelectBuilder IFilterable<ISelectBuilder>.If(bool condition, Action<ISelectBuilder> action) => this.InternalIf(condition, action);

    IDeleteBuilder IFilterable<IDeleteBuilder>.If(bool condition, Action<IDeleteBuilder> action) => this.InternalIf(condition, action);
    IUpdateBuilder IFilterable<IUpdateBuilder>.If(bool condition, Action<IUpdateBuilder> action) => this.InternalIf(condition, action);

    SqlBuilderResult IQueryBuilder.Build() => this.InternalBuild();
    SqlBuilderResult ISelectBuilder.Build() => this.InternalBuild();
    SqlBuilderResult IInsertBuilder.Build() => this.InternalBuild();
    SqlBuilderResult IDeleteBuilder.Build() => this.InternalBuild();
    SqlBuilderResult IUpdateBuilder.Build() => this.InternalBuild();
    SqlBuilderResult IQueryBuilder.BuildCount() => this.InternalBuildCount();
    SqlBuilderResult ISelectBuilder.BuildCount() => this.InternalBuildCount();

    public IQueryBuilder Select(string select, Type type = null)
    {
        if (!string.IsNullOrWhiteSpace(select))
        {
            _selects.Add(select);
        }

        if (type != null)
        {
            this._types.Add(type);
        }

        return this;
    }

    public IQueryBuilder GroupBy(string clause)
    {
        _groupBy = clause;
        return this;
    }

    public IQueryBuilder OrderBy(string clause)
    {
        _orderBy = clause;
        return this;
    }

    public IQueryBuilder OrderByRandom()
    {
        _orderBy = _dialect.RandomFunction;
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

    public IQueryBuilder InnerJoin(string table, string alias, string on, string splitColumn = "Id")
    {
        var sql = $"INNER JOIN {table} {alias} ON {on}";
        _joinMetadata.Add(new JoinMetadata(sql, splitColumn));
        return this;
    }

    public IQueryBuilder LeftJoin(string table, string alias, string on, string splitColumn = "Id")
    {
        var sql = $"LEFT JOIN {table} {alias} ON {on}";
        _joinMetadata.Add(new JoinMetadata(sql, splitColumn));
        return this;
    }

    public IQueryBuilder RightJoin(string table, string alias, string on, string splitColumn = "Id")
    {
        var sql = $"RIGHT JOIN {table} {alias} ON {on}";
        _joinMetadata.Add(new JoinMetadata(sql, splitColumn));
        return this;
    }

    public IQueryBuilder Where(string condition, object param = null)
    {
        this.InternalWhere(condition, param);
        return this;
    }

    public IQueryBuilder WhereEquals(string column, object value)
    {
        return AddComparison(column, "=", value);
    }

    public IQueryBuilder WhereNotEquals(string column, object value)
    {
        return AddComparison(column, "!=", value);
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

        if (_isUpdate)
        {
            sb.Append($"UPDATE {this._from} SET {string.Join(", ", _updates)}");
        }
        else if (_isInsert)
        {
            return this.BuildInsertStatement();
        }
        else if (_isDelete)
        {
            sb.Append($"DELETE FROM {this._from}");
        }
        else
        {
            var selectClause = this._selects.Any() ? string.Join(", ", this._selects) : $"{_from}.*";
            sb.Append($"SELECT {selectClause} FROM {_from} {string.Join(" ", _joinMetadata.Select(x => x.JoinSql))}");
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
                sb.Append(_dialect.GetPaginationSql(PageSize, offset));
            }
        }

        var sql = sb.ToString();
        var joinMetadata = this._joinMetadata.ToList();
        var splitColumns = joinMetadata.Select(x => x.SplitColumn).ToList();
        var splitOn = splitColumns.Count > 0 ? string.Join(",", splitColumns) : "Id";
        Debug.WriteLine(sql);
        return new SqlBuilderResult(sql, _params, this._types.ToArray(), splitOn);
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

    private SqlBuilderResult BuildInsertStatement()
    {
        var columns = string.Join(", ", _insertColumns);
        var values = string.Join(", ", _insertColumns.Select(p => $"@{p}"));

        var sb = new StringBuilder();
        sb.Append($"INSERT INTO {this._from} ({columns}) VALUES ({values})");

        if (_isUpsert)
        {
            sb.Append(_dialect.GetUpsertSql(_from, _insertColumns, _keyColumns));
        }

        sb.Append(";");

        sb.Append(_dialect.GetIdentitySql());

        Debug.WriteLine(sb.ToString());
        return new SqlBuilderResult(sb.ToString(), _params, null, null);
    }

    private void InternalWhere(string condition, object param = null)
    {
        _wheres.Add(condition);
        if (param is not null)
        {
            _params.AddDynamicParams(param);
        }
    }

    private IQueryBuilder AddComparison(string column, string operatorSymbol, object value)
    {
        if (value is null)
        {
            var nullOp = operatorSymbol == "=" ? "IS NULL" : "IS NOT NULL";
            _wheres.Add($"{column} {nullOp}");
            return this;
        }

        var paramName = column.Replace(".", "_");

        _wheres.Add($"{column} {operatorSymbol} @{paramName}");
        _params.Add(paramName, value);

        return this;
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