using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;

namespace FivePD.Server.Database.Sql.Migration;

#pragma warning disable

public class TableBuilder
{
    private readonly IDbConnection _connection;
    private readonly DatabaseType _dbType;
    private readonly string _tableName;
    private readonly List<ColumnBuilder> _columns = new();
    private readonly List<ForeignKeyBuilder> _foreignKeys = new();
    private readonly List<(string Name, string Column)> _indexes = new();
    private string[] _compositePrimaryKeys;

    public TableBuilder(IDbConnection connection, DatabaseType dbType, string tableName)
    {
        this._connection = connection;
        this._dbType = dbType;
        this._tableName = tableName;
    }

    public TableBuilder Column(string name, Action<ColumnBuilder> setup)
    {
        var column = new ColumnBuilder(name, this._dbType);
        setup(column);
        this._columns.Add(column);
        return this;
    }
    
    public TableBuilder PrimaryKey(params string[] columns)
    {
        this._compositePrimaryKeys = columns;
        return this;
    }

    public TableBuilder ForeignKey(string localCol, Action<ForeignKeyBuilder> setup)
    {
        var fk = new ForeignKeyBuilder(localCol);
        setup(fk);
        this._foreignKeys.Add(fk);
        return this;
    }

    public TableBuilder Index(string indexName, string column)
    {
        this._indexes.Add((indexName, column));
        return this;
    }

    public void Execute()
    {
        var definitions = this._columns.Select(c => c.Build()).ToList();

        // Add Composite Primary Key if defined
        if (this._compositePrimaryKeys != null && this._compositePrimaryKeys.Length > 0)
        {
            var keys = string.Join(", ", this._compositePrimaryKeys.Select(k => $"`{k}`"));
            definitions.Add($"PRIMARY KEY ({keys})");
        }

        // Add Foreign Keys
        definitions.AddRange(this._foreignKeys.Select(f => f.Build()));

        var createSql = $@"CREATE TABLE IF NOT EXISTS `{this._tableName}` (
            {string.Join(",\n            ", definitions)}
        );";

        this._connection.Execute(createSql);

        foreach (var (name, col) in this._indexes)
        {
            string indexSql = this._dbType == DatabaseType.SQLite 
                ? $"CREATE INDEX IF NOT EXISTS `{name}` ON `{this._tableName}` (`{col}`);"
                : $"CREATE INDEX `{name}` ON `{this._tableName}` (`{col}`);"; 
            
            try { this._connection.Execute(indexSql); } catch { /* Index exist handling */ }
        }
    }
}