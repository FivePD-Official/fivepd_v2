using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Dapper;
using Serilog;

namespace FivePD.Server.Database.Migrations;

/// <summary>
/// Runs database migrations automatically, similar to EF Core.
/// </summary>
public class MigrationRunner
{
    private readonly IDatabaseContextFactory _dbContextFactory;
    private readonly ILogger _logger;

    public MigrationRunner(IDatabaseContextFactory dbContextFactory, ILogger logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Applies all pending migrations automatically.
    /// </summary>
    public void ApplyMigrations()
    {
        _logger?.Information("Starting migration process...");

        // Create migrations table if it doesn't exist
        CreateMigrationsTable();

        // Get all migrations from assembly
        var migrations = GetMigrations();
        
        if (!migrations.Any())
        {
            _logger?.Warning("No migrations found!");
            return;
        }

        _logger?.Information($"Found {migrations.Count} migrations");

        // Apply pending migrations
        using var scope = _dbContextFactory.CreateScope();
        var connection = scope.Connection;

        foreach (var migration in migrations)
        {
            if (!IsMigrationApplied(connection, migration.Id))
            {
                _logger?.Information($"Applying migration: {migration.Id} - {migration.Description}");
                
                try
                {
                    migration.Up(connection);
                    MarkMigrationAsApplied(connection, migration.Id, migration.Description);
                    _logger?.Information($"✓ Applied migration: {migration.Id}");
                }
                catch (Exception ex)
                {
                    _logger?.Error($"✗ Failed to apply migration {migration.Id}: {ex.Message}");
                    throw;
                }
            }
            else
            {
                _logger?.Debug($"Migration {migration.Id} already applied, skipping.");
            }
        }

        _logger?.Information("All migrations completed successfully!");
    }

    /// <summary>
    /// Reverts a specific migration.
    /// </summary>
    public void RevertMigration(string migrationId)
    {
        _logger?.Information($"Reverting migration: {migrationId}");

        using var scope = _dbContextFactory.CreateScope();
        var connection = scope.Connection;

        var migration = GetMigrations().FirstOrDefault(m => m.Id == migrationId);
        if (migration == null)
        {
            throw new InvalidOperationException($"Migration {migrationId} not found");
        }

        if (!IsMigrationApplied(connection, migrationId))
        {
            throw new InvalidOperationException($"Migration {migrationId} is not applied");
        }

        try
        {
            migration.Down(connection);
            UnmarkMigration(connection, migrationId);
            _logger?.Information($"✓ Reverted migration: {migrationId}");
        }
        catch (Exception ex)
        {
            _logger?.Error($"✗ Failed to revert migration {migrationId}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Lists all applied migrations.
    /// </summary>
    public List<AppliedMigration> GetAppliedMigrations()
    {
        using var scope = _dbContextFactory.CreateScope();
        var connection = scope.Connection;

        return connection.Query<AppliedMigration>(
            "SELECT Id, Description, AppliedAt FROM __Migrations ORDER BY AppliedAt").ToList();
    }

    private void CreateMigrationsTable()
    {
        using var scope = _dbContextFactory.CreateScope();
        var connection = scope.Connection;

        // Check if table exists (provider-specific)
        string checkSql;
        string createSql;

        if (connection is System.Data.SQLite.SQLiteConnection)
        {
            checkSql = "SELECT name FROM sqlite_master WHERE type='table' AND name='__Migrations'";
            createSql = @"
                CREATE TABLE IF NOT EXISTS __Migrations (
                    Id TEXT PRIMARY KEY,
                    Description TEXT NOT NULL,
                    AppliedAt DATETIME NOT NULL
                )";
        }
        else
        {
            // MySQL/MariaDB
            checkSql = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = '__Migrations'";
            createSql = @"
                CREATE TABLE IF NOT EXISTS __Migrations (
                    Id VARCHAR(100) PRIMARY KEY,
                    Description VARCHAR(255) NOT NULL,
                    AppliedAt DATETIME NOT NULL
                )";
        }

        var exists = connection.ExecuteScalar<int>(checkSql) > 0;
        if (!exists)
        {
            connection.Execute(createSql);
            _logger?.Debug("Created __Migrations table");
        }
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
            "SELECT COUNT(*) FROM __Migrations WHERE Id = @Id", 
            new { Id = migrationId });
        return count > 0;
    }

    private void MarkMigrationAsApplied(IDbConnection connection, string migrationId, string description)
    {
        connection.Execute(
            "INSERT INTO __Migrations (Id, Description, AppliedAt) VALUES (@Id, @Description, @AppliedAt)",
            new { Id = migrationId, Description = description, AppliedAt = DateTime.UtcNow });
    }

    private void UnmarkMigration(IDbConnection connection, string migrationId)
    {
        connection.Execute("DELETE FROM __Migrations WHERE Id = @Id", new { Id = migrationId });
    }
}

/// <summary>
/// Represents an applied migration record.
/// </summary>
public class AppliedMigration
{
    public string Id { get; set; }
    public string Description { get; set; }
    public DateTime AppliedAt { get; set; }
}