using FivePD.Server.Interfaces;
using MySqlConnector;
using Serilog;

namespace FivePD.Server.Database;

/// <summary>
/// Factory for creating database connections using Dapper.
/// Supports MySQL, MariaDB, and SQLite.
/// </summary>
public class DapperConnectionFactory : IDapperDatabaseConnectionFactory
{
    private readonly ILogger _logger;
    private readonly IConfigService _configService;

    private readonly DatabaseType _databaseType;
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperConnectionFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger for logging messages.</param>
    /// <param name="configService">The configuration service to retrieve database settings.</param>
    public DapperConnectionFactory(ILogger logger, IConfigService configService)
    {
        this._logger = logger;
        this._configService = configService;
        (this._connectionString, this._databaseType) = this.BuildConnectionString();
    }

    /// <inheritdoc />
    public string ConnectionString => this._connectionString;

    /// <inheritdoc />
    public IDapperDatabaseConnectionScope CreateScope()
    {
        return new DapperConnectionScope(this._connectionString, this._databaseType);
    }

    /// <summary>
    /// Builds the connection string from the configuration.
    /// </summary>
    /// <returns>Tuple containing connection string and database type.</returns>
    private (string ConnectionString, DatabaseType DbType) BuildConnectionString()
    {
        var config = this._configService?.GeneralConfig?.Database;
        if (config is null)
        {
            this._logger.Fatal("Database configuration is not available.");
            LifecycleHandler.Stop();
            return (null, DatabaseType.MySql);
        }

#pragma warning disable
        var type = config.Type?.ToLower() ?? "mysql";
        // if (provider.Contains("sqlite"))
        // {
        //     // SQLite connection
        //     var dbPath = config.DatabaseName ?? "fivepd.db";
        //     if (!System.IO.Path.IsPathRooted(dbPath))
        //     {
        //         // Make path relative to server data directory
        //         dbPath = System.IO.Path.Combine("data", dbPath);
        //     }
        //     
        //     // Ensure directory exists
        //     var directory = System.IO.Path.GetDirectoryName(dbPath);
        //     if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
        //     {
        //         System.IO.Directory.CreateDirectory(directory);
        //     }
        //
        //     var sqliteBuilder = new SQLiteConnectionStringBuilder
        //     {
        //         DataSource = dbPath,
        //         Pooling = true,
        //         MaxPoolSize = 100,
        //         DefaultTimeout = 30
        //     };
        //
        //     return (sqliteBuilder.ToString(), DatabaseType.SQLite);
        // }
#pragma warning restore

        // MySQL/MariaDB connection
        var builder = new MySqlConnectionStringBuilder
        {
            Server = config.Address,
            Port = config.Port,
            UserID = config.User,
            Password = config.Password,
            Database = config.Name,
            Pooling = true,
            MinimumPoolSize = 1,
            MaximumPoolSize = 100,
            ConnectionTimeout = 30,
            AllowUserVariables = true,
        };

        return (builder.ToString(), type.Contains("mariadb") ? DatabaseType.MariaDb : DatabaseType.MySql);
    }
}