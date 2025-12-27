using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using FivePD.Common.ConfigModels;
using FivePD.Server.Database.Migrations;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Models.LoadoutModels.JoinTables;
using FivePD.Server.Utilities;
using Serilog;

namespace FivePD.Server.Database;

/// <summary>
/// Handles database schema creation and migrations for Dapper.
/// Uses MigrationRunner for automatic migration application.
/// </summary>
public class DatabaseMigrationManager
{
    private readonly IDatabaseContextFactory _dbContextFactory;
    private readonly ILogger _logger;
    private readonly IConfigService _configService;
    private readonly MigrationRunner _migrationRunner;

    public DatabaseMigrationManager(
        IDatabaseContextFactory dbContextFactory,
        ILogger logger,
        IConfigService configService)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _configService = configService;
        _migrationRunner = new MigrationRunner(dbContextFactory, logger);
    }

    /// <summary>
    /// Ensures the database schema exists and is up to date using migrations.
    /// </summary>
    public void EnsureDatabaseSchema()
    {
        try
        {
            // Apply all pending migrations automatically
            _migrationRunner.ApplyMigrations();

            // Verify connection
            using var scope = _dbContextFactory.CreateScope();
            var connection = scope.Connection;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            _logger?.Information("Database schema is up to date and ready.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to apply database migrations. Verify your database configuration.", ex);
        }
    }

    /// <summary>
    /// Lists all applied migrations.
    /// </summary>
    public void ListAppliedMigrations()
    {
        var migrations = _migrationRunner.GetAppliedMigrations();
        
        if (!migrations.Any())
        {
            _logger?.Information("No migrations have been applied.");
            return;
        }

        _logger?.Information("Applied Migrations:");
        foreach (var migration in migrations)
        {
            _logger?.Information($"  - {migration.Id}: {migration.Description} (Applied: {migration.AppliedAt:yyyy-MM-dd HH:mm:ss})");
        }
    }

    /// <summary>
    /// Reverts a specific migration.
    /// </summary>
    public void RevertMigration(string migrationId)
    {
        try
        {
            _migrationRunner.RevertMigration(migrationId);
            _logger?.Information($"Successfully reverted migration: {migrationId}");
        }
        catch (Exception ex)
        {
            _logger?.Error($"Failed to revert migration {migrationId}: {ex.Message}");
            throw;
        }
    }
}