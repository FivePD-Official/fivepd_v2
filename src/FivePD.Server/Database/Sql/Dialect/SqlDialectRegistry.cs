using System;
using FivePD.Server.Database.Sql.Dialect;

namespace FivePD.Server.Database.Sql.Dialect;

#pragma warning disable

public static class SqlDialectRegistry
{
    private static ISqlDialect _current;
    public static ISqlDialect Current => _current ?? throw new InvalidOperationException("SqlBuilder Dialect not initialized.");

    public static void SetDialect(DatabaseType type)
    {
        _current = type switch
        {
            DatabaseType.MySql => new MySqlDialect(),
            DatabaseType.SQLite => new SqliteDialect(),
            _ => new MySqlDialect()
        };
    }
}
