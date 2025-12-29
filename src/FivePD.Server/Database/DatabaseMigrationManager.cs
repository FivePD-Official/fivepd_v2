using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Dapper;
using FivePD.Server.Database.Migrations;
using FivePD.Server.Interfaces;
using Serilog;

namespace FivePD.Server.Database;

/// <summary>
/// Handles database schema creation and migrations for Dapper.
/// Uses MigrationRunner for automatic migration application.
/// </summary>
public class DatabaseMigrationManager
{
    private readonly IDapperDatabaseConnectionFactory _dbConnectionFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseMigrationManager"/> class.
    /// Manages automatic database schema creation, migrations, and migration-related operations such as
    /// retrieving applied migrations and reverting migrations.
    /// </summary>
    /// <param name="dbConnectionFactory">The <see cref="IDapperDatabaseConnectionFactory"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public DatabaseMigrationManager(IDapperDatabaseConnectionFactory dbConnectionFactory, ILogger logger)
    {
        this._dbConnectionFactory = dbConnectionFactory;
        this._logger = logger;
    }

    /// <summary>
    /// Applies all pending migrations automatically.
    /// </summary>
    public void ApplyMigrations()
    {
        this._logger.Information("Starting migration process...");

        this.CreateMigrationsTable();

        var migrations = this.GetMigrations();
        if (!migrations.Any())
        {
            this._logger.Warning("No migrations found!");
            return;
        }

        this._logger.Information("Found {migrationsCount} migration(s)", migrations.Count);

        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        foreach (var migration in migrations)
        {
            if (this.IsMigrationApplied(connection, migration.Id))
            {
                this._logger.Debug("Migration {migrationId} already applied, skipping.", migration.Id);
                continue;
            }

            try
            {
                this._logger.Information("Applying migration: {migrationId} - {migrationDescription}", migration.Id, migration.Description);
                migration.Up(connection);
                this.MarkMigrationAsApplied(connection, migration.Id, migration.Description);
                this._logger.Information("Applied migration: {migrationId}", migration.Id);
            }
            catch (Exception ex)
            {
                this._logger.Error("Failed to apply migration {migrationId}: {exMessage}", migration.Id, ex.Message);
                this.RevertMigration(migration.Id, true);
            }
        }

        this._logger.Information("All migrations completed successfully!");
    }

    /// <summary>
    /// Reverts a previously applied database migration specified by its unique ID.
    /// Ensures the migration is present, has been applied, and reverts it if these conditions are met.
    /// Logs the operation and rethrows any exceptions encountered during the operation.
    /// </summary>
    /// <param name="migrationId">The unique identifier of the migration to revert.</param>
    /// <param name="revertFailedMigration">Whether to revert a failed migration. If false only applies to successful migrations.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the migration with the specified ID is not found or has not been applied.
    /// </exception>
    public void RevertMigration(string migrationId, bool revertFailedMigration)
    {
        this._logger.Information("Reverting migration: {migrationId}", migrationId);

        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var migration = this.GetMigrations().FirstOrDefault(m => m.Id == migrationId);
        if (migration is null)
        {
            this._logger.Warning("Migration {migrationId} not found, cannot revert", migrationId);
            return;
        }

        if (!revertFailedMigration && !this.IsMigrationApplied(connection, migrationId))
        {
            this._logger.Warning("Cannot revert migration {migrationId}, it has not been applied", migrationId);
        }

        try
        {
            migration.Down(connection);
            this.UnmarkMigration(connection, migrationId);
            this._logger.Information("Reverted migration: {migrationId}", migrationId);
        }
        catch (Exception ex)
        {
            this._logger.Error("Failed to revert migration {migrationId}: {exMessage}", migrationId, ex.Message);
        }
    }

    private void CreateMigrationsTable()
    {
        using var connection = this._dbConnectionFactory.CreateScope().Connection;

        string checkSql;
        string createSql;

#pragma warning disable
        // if (connection is System.Data.SQLite.SQLiteConnection)
        // {
        //     checkSql = "SELECT name FROM sqlite_master WHERE type='table' AND name='__Migrations'";
        //     createSql = @"
        //         CREATE TABLE IF NOT EXISTS __Migrations (
        //             Id TEXT PRIMARY KEY,
        //             Description TEXT NOT NULL,
        //             AppliedAt DATETIME NOT NULL
        //         )";
        // }
#pragma warning restore

        // MySQL/MariaDB
        checkSql = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = '{TableNames.FivePdMigrations}'";
        createSql = $@"
            CREATE TABLE IF NOT EXISTS {TableNames.FivePdMigrations} (
                Id VARCHAR(100) PRIMARY KEY,
                Description VARCHAR(255) NOT NULL,
                AppliedAt DATETIME NOT NULL
            )";

        var exists = connection.ExecuteScalar<int>(checkSql) > 0;
        if (exists)
        {
            return;
        }

        connection.Execute(createSql);
        this._logger.Information("Created __fivepd_migrations table");
    }

    private List<IMigration> GetMigrations()
    {
        var migrationType = typeof(IMigration);
        var assembly = Assembly.GetExecutingAssembly();

        return assembly.GetTypes()
            .Where(t => migrationType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(t => (IMigration)Activator.CreateInstance(t))
            .OrderBy(m => m.Id)
            .ToList();
    }

    private bool IsMigrationApplied(IDbConnection connection, string migrationId)
    {
        var count = connection.ExecuteScalar<int>(
            $"SELECT COUNT(*) FROM {TableNames.FivePdMigrations} WHERE Id = @Id",
            new { Id = migrationId });
        return count > 0;
    }

    private void MarkMigrationAsApplied(IDbConnection connection, string migrationId, string description)
    {
        connection.Execute(
            $"INSERT INTO {TableNames.FivePdMigrations} (Id, Description, AppliedAt) VALUES (@Id, @Description, @AppliedAt)",
            new { Id = migrationId, Description = description, AppliedAt = DateTime.UtcNow });
    }

    private void UnmarkMigration(IDbConnection connection, string migrationId)
    {
        connection.Execute($"DELETE FROM {TableNames.FivePdMigrations} WHERE Id = @Id", new { Id = migrationId });
    }
}

#pragma warning disable
/// <summary>
/// Represents an applied migration record.
/// </summary>
public class AppliedMigration
{
    public string Id { get; set; }

    public string Description { get; set; }

    public DateTime AppliedAt { get; set; }
}
#pragma warning restore