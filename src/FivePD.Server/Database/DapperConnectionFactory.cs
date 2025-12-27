using System;
using System.Data;
using System.Data.SQLite;
using FivePD.Common.ConfigModels;
using FivePD.Server.Interfaces;
using MySqlConnector;

namespace FivePD.Server.Database;

/// <summary>
/// Factory for creating database connections using Dapper.
/// Supports MySQL, MariaDB, and SQLite.
/// Replaces the EF Core DatabaseContext.
/// </summary>
public class DapperConnectionFactory : IDatabaseContextFactory
{
    private readonly IConfigService _configService;
    private readonly string _connectionString;
    private readonly DatabaseType _databaseType;

    /// <summary>
    /// Database types supported by the factory.
    /// </summary>
    public enum DatabaseType
    {
        MySQL,
        MariaDB,
        SQLite
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperConnectionFactory"/> class.
    /// </summary>
    /// <param name="configService">The configuration service to retrieve database settings.</param>
    public DapperConnectionFactory(IConfigService configService)
    {
        _configService = configService;
        (_connectionString, _databaseType) = BuildConnectionString();
    }

    /// <summary>
    /// Creates a new database connection scope.
    /// </summary>
    /// <returns>A new database context scope.</returns>
    public IDatabaseContextScope CreateScope()
    {
        return new DapperContextScope(_connectionString, _databaseType);
    }

    /// <summary>
    /// Builds the connection string from the configuration.
    /// </summary>
    /// <returns>Tuple containing connection string and database type.</returns>
    private (string connectionString, DatabaseType dbType) BuildConnectionString()
    {
        var config = _configService?.GeneralConfig?.Database;
        if (config is null)
        {
            throw new InvalidOperationException("Database configuration is not available.");
        }

        // Determine database type from provider or connection string
        var provider = config.Provider?.ToLower() ?? "mysql";
        
        if (provider.Contains("sqlite"))
        {
            // SQLite connection
            var dbPath = config.DatabaseName ?? "fivepd.db";
            if (!System.IO.Path.IsPathRooted(dbPath))
            {
                // Make path relative to server data directory
                dbPath = System.IO.Path.Combine("data", dbPath);
            }
            
            // Ensure directory exists
            var directory = System.IO.Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            var sqliteBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                Pooling = true,
                MaxPoolSize = 100,
                DefaultTimeout = 30
            };

            return (sqliteBuilder.ToString(), DatabaseType.SQLite);
        }
        else
        {
            // MySQL/MariaDB connection
            var builder = new MySqlConnectionStringBuilder
            {
                Server = config.Host ?? "localhost",
                Port = config.Port ?? 3306,
                UserID = config.Username ?? "root",
                Password = config.Password ?? "",
                Database = config.DatabaseName ?? "fivepd",
                Pooling = true,
                MinimumPoolSize = 1,
                MaximumPoolSize = 100,
                ConnectionTimeout = 30,
                CommandTimeout = 30,
                AllowUserVariables = true
            };

            // MariaDB specific optimizations
            if (provider.Contains("mariadb"))
            {
                builder.Server = config.Host ?? "localhost";
            }

            return (builder.ToString(), provider.Contains("mariadb") ? DatabaseType.MariaDB : DatabaseType.MySQL);
        }
    }
}