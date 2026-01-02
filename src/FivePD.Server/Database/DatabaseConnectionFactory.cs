using System;
using System.Data.SQLite;
using System.IO;
using System.Runtime.InteropServices;
using FivePD.Server.Interfaces;
using MySqlConnector;
using Serilog;
using SQLitePCL;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Database;

#pragma warning disable
public static class SqliteNativeLoader
{
    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string libname);

    public static void ForceLoad(string dllPath)
    {
        IntPtr handle = LoadLibrary(dllPath);
        if (handle == IntPtr.Zero)
        {
            int errorCode = Marshal.GetLastWin32Error();
            throw new Exception($"Failed to load e_sqlite3.dll from {dllPath}. Win32 Error: {errorCode}");
        }
    }
}
#pragma warning restore

/// <summary>
/// Factory for creating database connections using Dapper.
/// Supports MySQL, MariaDB, and SQLite.
/// </summary>
public class DatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly ILogger _logger;
    private readonly IConfigService _configService;

    private readonly DatabaseType _databaseType;
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseConnectionFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger for logging messages.</param>
    /// <param name="configService">The configuration service to retrieve database settings.</param>
    public DatabaseConnectionFactory(ILogger logger, IConfigService configService)
    {
        this._logger = logger;
        this._configService = configService;

        this.LoadSqliteDll();
        (this._connectionString, this._databaseType) = this.BuildConnectionString();
    }

    /// <inheritdoc />
    public string ConnectionString => this._connectionString;

    /// <inheritdoc />
    public IDatabaseConnectionScope CreateScope()
    {
        return new DatabaseConnectionScope(this._connectionString, this._logger, this._databaseType);
    }

    private void LoadSqliteDll()
    {
        if (this._configService.DatabaseType != DatabaseType.SQLite)
        {
            return;
        }

        try
        {
            var dir = Cfx.API.GetResourcePath(Cfx.API.GetCurrentResourceName());
            var path = Path.Combine(dir, "Libraries", "FivePD.Server", "runtimes", "win-x64", "native", "e_sqlite3.dll");
            SqliteNativeLoader.ForceLoad(path);
            Batteries_V2.Init();
        }
        catch (Exception exception)
        {
            this._logger.Fatal("Could not initialize SQLite. {exception}", exception);
            LifecycleHandler.Stop();
        }
    }

    /// <summary>
    /// Builds the connection string from the configuration.
    /// </summary>
    /// <returns>Tuple containing connection string and database type.</returns>
    private (string ConnectionString, DatabaseType DatabaseType) BuildConnectionString()
    {
        if (this._configService.DatabaseType == DatabaseType.SQLite)
        {
            var dir = Cfx.API.GetResourcePath(Cfx.API.GetCurrentResourceName());
            var sqliteBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = Path.Combine(dir, $"{this._configService.GeneralConfig.Database.Name}.db"),
                Pooling = true,
            };

            return (sqliteBuilder.ToString(), DatabaseType.SQLite);
        }

        var config = this._configService.GeneralConfig.Database;
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

        return (builder.ToString(), this._configService.DatabaseType);
    }
}