using System;
using System.Data;
using Dapper;
using FivePD.Server.Database.Sql.Dialect;
using FivePD.Server.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Serilog;

namespace FivePD.Server.Database;

#pragma warning disable

/// <summary>
/// Handles database operations and ensures the correct setup and migration of the database using Dapper.
/// Replaces the EF Core DatabaseHandler.
/// </summary>
internal static class DatabaseHandler
{
    private static ServiceProvider _provider;
    private static ILogger _logger;
    private static IDatabaseConnectionFactory _dbConnectionFactory;
    private static DatabaseMigrationManager _migrationManager;
    private static IConfigService _configService;

    /// <summary>
    /// Sets the parameters of the current instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider to be used.</param>
    /// <param name="logger">The logger to be used.</param>
    public static void SetParameters(ServiceProvider serviceProvider, ILogger logger)
    {
        _provider = serviceProvider;
        _logger = logger;
        _dbConnectionFactory = serviceProvider.GetService<IDatabaseConnectionFactory>();
        _configService = serviceProvider.GetService<IConfigService>();

        if (_dbConnectionFactory is null)
        {
            _logger.Fatal("No database connection factory is available for usage.");
            LifecycleHandler.Stop();
        }

        _migrationManager = new DatabaseMigrationManager(_dbConnectionFactory, logger, _configService.DatabaseType);

        DapperTypeHandlers.Register();
        _logger.Information("Dapper type handlers registered successfully");
    }

    public static void EnsureDatabaseConfig()
    {
        var config = _configService.GeneralConfig.Database;
        if (config is null)
        {
            Shutdown("Database configuration is not available.");
            return;
        }

        if (string.IsNullOrWhiteSpace(config.Type))
        {
            Shutdown("Database type is not specified.");
            return;
        }

        var type = config.Type.ToLower();
        if (type != "sqlite" && type != "mysql" && type != "mariadb")
        {
            Shutdown("Unsupported database type \"{configType}\" specified in configuration.", config.Type);
            return;
        }

        if (string.IsNullOrWhiteSpace(config.Name))
        {
            Shutdown("Database name is not specified.");
            return;
        }

        if (_configService.DatabaseType is DatabaseType.MySql or DatabaseType.MariaDb)
        {
            if (string.IsNullOrWhiteSpace(config.Address))
            {
                Shutdown("Database address is missing.");
            }

            if (config.Port <= 0)
            {
                Shutdown("Invalid database port.");
            }

            if (string.IsNullOrWhiteSpace(config.User))
            {
                Shutdown("Database user is missing.");
            }

            if (string.IsNullOrWhiteSpace(config.Password))
            {
                Shutdown("Database password is missing.");
            }
        }

        _logger.Information("Database configuration is valid. Selected database type: {DatabaseType}", _configService.DatabaseType);
        SqlDialectRegistry.SetDialect(_configService.DatabaseType);
        return;

        void Shutdown(string message, params object[] parameters)
        {
            _logger.Fatal(message, parameters);
            LifecycleHandler.Stop();
        }
    }

    /// <summary>
    /// Ensures that the database exists. Creates it if it doesn't.
    /// </summary>
    public static void EnsureDatabaseExists()
    {
        try
        {
            var factory = _dbConnectionFactory;
            if (factory is null)
            {
                _logger.Warning("Unable to determine database factory type, skipping database creation check");
                return;
            }

            if (_configService.DatabaseType == DatabaseType.SQLite)
            {
                _logger.Information("SQLite database will be created automatically when needed");
                return;
            }

            var builder = new MySqlConnectionStringBuilder(factory.ConnectionString);
            var databaseName = builder.Database;

            if (string.IsNullOrEmpty(databaseName))
            {
                _logger.Warning("Database name not specified in configuration");
                return;
            }

            builder.Database = null!;
            var serverConnectionString = builder.ToString();

            using var connection = new MySqlConnection(serverConnectionString);
            connection.Open();

            var dbExists = connection.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = @DatabaseName",
                new { DatabaseName = databaseName }) > 0;
            if (dbExists)
            {
                _logger.Information("Database \"{databaseName}\" already exists", databaseName);
            }
            else
            {
                _logger.Information("Database \"{databaseName}\" does not exist. Creating...", databaseName);
                var createDbSql = $"CREATE DATABASE IF NOT EXISTS `{databaseName}`";
                connection.Execute(createDbSql);
                _logger.Information("Database \"{databaseName}\" created successfully", databaseName);
            }
        }
        catch (Exception)
        {
            _logger.Error("Failed to ensure database exists. Verify the connection settings in the /Config/general file and try again.");
        }
    }

    /// <summary>
    /// Ensures that a connection to the database is established and open.
    /// If the connection is not open, it attempts to open it.
    /// Logs the readiness of the database if successful or logs an error
    /// if the database is offline or misconfigured.
    /// </summary>
    public static void EnsureConnection()
    {
        try
        {
            using var scope = _dbConnectionFactory.CreateScope();
            if (scope.Connection.State != ConnectionState.Open)
            {
                scope.Connection.Open();
            }

            _logger.Information("Database is ready");
        }
        catch (Exception)
        {
            _logger.Error("The configured database is invalid or offline. Verify the connection settings in the /Config/general file and try again.");
        }
    }

    /// <summary>
    /// Ensures the database schema is up to date by applying any pending migrations.
    /// Logs success or failure of the operation.
    /// </summary>
    /// <exception cref="Exception">
    /// Thrown if an error occurs while applying database migrations. Logs the error to the configured logger.
    /// </exception>
    public static void EnsureSchema()
    {
        try
        {
            _migrationManager.ApplyMigrations();
            _logger.Information("Database schema is up to date");
        }
        catch (Exception exception)
        {
            _logger.Error("Failed to apply database migrations. Verify your database configuration. {e}", exception);
        }
    }

    /// <summary>
    /// Updates the database with a sample department if there are none
    /// and updates the callouts.
    /// </summary>
    public static void SeedData()
    {
        new SeedDataHandler(_provider, _logger, _dbConnectionFactory).SeedData();
    }
}