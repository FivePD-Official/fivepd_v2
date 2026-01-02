using System.Collections.Generic;
using FivePD.Server.Database.Sql.Dialect;

namespace FivePD.Server.Database.Sql.Migration;

#pragma warning disable

public class ColumnBuilder
{
    private readonly string _name;
    private string _type;
    private bool _isPrimaryKey;
    private bool _isAutoIncrement;
    private bool _isNotNull;
    private string _defaultValue;

    public ColumnBuilder(string name)
    {
        this._name = name;
    }

    public ColumnBuilder Integer()
    {
        this._type = "INT";
        return this;
    }

    public ColumnBuilder Text()
    {
        this._type = "TEXT";
        return this;
    }

    public ColumnBuilder Varchar(int length = 0)
    {
        this._type = SqlDialectRegistry.Type == DatabaseType.SQLite ? "TEXT" : $"VARCHAR({length})";
        return this;
    }

    public ColumnBuilder LongText()
    {
        this._type = SqlDialectRegistry.Type == DatabaseType.SQLite ? "TEXT" : "LONGTEXT";
        return this;
    }

    public ColumnBuilder Double()
    {
        this._type = "DOUBLE";
        return this;
    }
    
    public ColumnBuilder Float()
    {
        this._type = SqlDialectRegistry.Type == DatabaseType.SQLite ? "REAL" : "FLOAT";
        return this;
    }

    public ColumnBuilder DateTime()
    {
        this._type = SqlDialectRegistry.Type == DatabaseType.SQLite ? "TEXT" : "DATETIME";
        return this;
    }

    public ColumnBuilder Boolean()
    {
        this._type = SqlDialectRegistry.Type == DatabaseType.SQLite ? "INTEGER" : "TINYINT";
        return this;
    }

    public ColumnBuilder PrimaryKey()
    {
        this._isPrimaryKey = true;
        return this;
    }

    public ColumnBuilder AutoIncrement()
    {
        this._isAutoIncrement = true;
        return this;
    }

    public ColumnBuilder NotNull()
    {
        this._isNotNull = true;
        return this;
    }

    public ColumnBuilder Default(object value)
    {
        this._defaultValue = value is string s ? $"'{s}'" : value.ToString();
        return this;
    }

    public string Build()
    {
        // Handle the SQLite AUTOINCREMENT quirk immediately
        if (this._isAutoIncrement && SqlDialectRegistry.Type == DatabaseType.SQLite)
        {
            return $"`{this._name}` INTEGER PRIMARY KEY AUTOINCREMENT";
        }

        var parts = new List<string> { $"`{this._name}`", this._type };

        if (this._isPrimaryKey) parts.Add("PRIMARY KEY");
        if (this._isAutoIncrement) parts.Add("AUTO_INCREMENT"); // MySQL style
        if (this._isNotNull) parts.Add("NOT NULL");
        if (this._defaultValue != null) parts.Add($"DEFAULT {this._defaultValue}");

        return string.Join(" ", parts);
    }
}