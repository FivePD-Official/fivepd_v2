using System;
using System.Data;
using System.Data.SQLite;
using FivePD.Server.Interfaces;
using MySqlConnector;

namespace FivePD.Server.Database;

#pragma warning disable

/// <summary>
/// Represents a scope for database operations using Dapper.
/// Provides a database connection that can be used for CRUD operations.
/// </summary>
#pragma warning disable S3881
public class DapperConnectionScope : IDapperDatabaseConnectionScope
#pragma warning restore S3881
{
    private readonly string _connectionString;
    private readonly DatabaseType _databaseType;

    private IDbConnection _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperConnectionScope"/> class.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="databaseType">The type of database.</param>
    public DapperConnectionScope(string connectionString, DatabaseType databaseType)
    {
        this._connectionString = connectionString;
        this._databaseType = databaseType;
        this._disposed = false;
    }

    /// <summary>
    /// Gets the database connection for Dapper operations.
    /// </summary>
    public IDbConnection Connection
    {
        get
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(DapperConnectionScope));
            }

            if (this._connection is null)
            {
                switch (this._databaseType)
                {
                    case DatabaseType.SQLite:
                        this._connection = new SQLiteConnection(_connectionString);
                        break;
                    case DatabaseType.MySql:
                    case DatabaseType.MariaDb:
                    default:
                        this._connection = new MySqlConnection(this._connectionString);
                        break;
                }

                this._connection.Open();
            }

            return this._connection;
        }
    }

    /// <summary>
    /// Disposes the database connection and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (this._disposed)
        {
            return;
        }

        this._connection?.Dispose();
        this._connection = null;
        this._disposed = true;
    }
}