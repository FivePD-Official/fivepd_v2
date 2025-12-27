using System;
using System.Data;
using System.Data.SQLite;
using FivePD.Server.Interfaces;
using MySqlConnector;

namespace FivePD.Server.Database;

/// <summary>
/// Represents a scope for database operations using Dapper.
/// Provides a database connection that can be used for CRUD operations.
/// </summary>
public class DapperContextScope : IDatabaseContextScope
{
    private readonly string _connectionString;
    private readonly DapperConnectionFactory.DatabaseType _databaseType;
    private IDbConnection _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperContextScope"/> class.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="databaseType">The type of database.</param>
    public DapperContextScope(string connectionString, DapperConnectionFactory.DatabaseType databaseType)
    {
        _connectionString = connectionString;
        _databaseType = databaseType;
        _disposed = false;
    }

    /// <summary>
    /// Gets the database connection for Dapper operations.
    /// </summary>
    public IDbConnection Connection
    {
        get
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DapperContextScope));
            }

            if (_connection == null)
            {
                switch (_databaseType)
                {
                    case DapperConnectionFactory.DatabaseType.SQLite:
                        _connection = new SQLiteConnection(_connectionString);
                        break;
                    case DapperConnectionFactory.DatabaseType.MySQL:
                    case DapperConnectionFactory.DatabaseType.MariaDB:
                    default:
                        _connection = new MySqlConnection(_connectionString);
                        break;
                }
                
                _connection.Open();
            }

            return _connection;
        }
    }

    /// <summary>
    /// Disposes the database connection and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _connection?.Dispose();
        _connection = null;
        _disposed = true;
    }
}