using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Server.Database;
using FivePD.Server.Exceptions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Addons;
using FivePD.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FivePD.Server;

/// <summary>
/// Handles database operations and ensures the correct setup and migration of the database.
/// </summary>
internal static class DatabaseHandler
{
    private static ServiceProvider provider;
    private static ILogger logger;
    private static DatabaseContext databaseContext;

    /// <summary>
    /// Sets the parameters of the current instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider to be used.</param>
    /// <param name="logger">The logger to be used.</param>
    /// <exception cref="DatabaseUsageException">Thrown when no database context is available for usage.</exception>
    public static void SetParameters(ServiceProvider serviceProvider, ILogger logger)
    {
        provider = serviceProvider;
        DatabaseHandler.logger = logger;
        databaseContext = serviceProvider.GetService<DatabaseContext>();

        if (databaseContext is null)
        {
            throw new DatabaseUsageException("No database context is available for usage.");
        }
    }

    /// <summary>
    /// Ensures that the database is in the correct state.
    /// </summary>
    public static void EnsureCorrectState()
    {
        try
        {
            Debug.WriteLine("EnsureCorrectState --------");

            var migrations = databaseContext.Database.GetPendingMigrations().ToList();
            Debug.WriteLine("EnsureCorrectState -------- after GetPendingMigrations");
            var databaseCreated = databaseContext.Database.EnsureCreated();
            Debug.WriteLine("EnsureCorrectState -------- after EnsureCreated");
            if (!databaseContext.Database.CanConnect())
            {
                Debug.WriteLine("EnsureCorrectState -------- inside CanConnect");
                throw new DatabaseUsageException("Database connection failed");
            }

            Debug.WriteLine("EnsureCorrectState -------- after CanConnect");

            switch (databaseCreated)
            {
                case true:
                {
                    logger?.Information("The database did not exist, it has been created");

                    var configService = provider.GetService<IConfigService>();
                    var config = configService?.GeneralConfig?.Database;

                    var tableName = string.Empty;
                    if (config is not null)
                    {
                        tableName = config.IsMariaDb() ? "__EFMigrationsHistory" : "__efmigrationshistory";
                    }

                    if (string.IsNullOrEmpty(tableName))
                    {
                        logger?.Fatal("The database type is not 'mariadb' or 'mysql'. Verify the settings in the /Config/general file and try again.");
                        LifecycleHandler.Stop();
                        return;
                    }

                    databaseContext.Database.ExecuteSqlRaw(
                        $"CREATE TABLE `{tableName}` (MigrationId VARCHAR(95) NOT NULL PRIMARY KEY, ProductVersion VARCHAR(32) NOT NULL)");

                    foreach (var migration in migrations)
                    {
                        databaseContext.Database.ExecuteSqlRaw($"INSERT INTO `{tableName}` (MigrationId, ProductVersion) VALUES ('{migration}', '3.1.26')");
                    }

                    break;
                }

                case false when migrations.Count > 0:
                    logger?.Information("Upgrading the database to {Version}", migrations.Last());
                    databaseContext.Database.Migrate();
                    break;
            }

            logger?.Information("The database is up to date");
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException(
                "The configured database is invalid or offline. Verify the connection settings in the /Config/general file and try again.", e);
        }
    }

    /// <summary>
    /// Updates the database with a sample department if there are none
    /// and updates the callouts.
    /// </summary>
    public static void Update()
    {
        UpdateCallouts();
        AddSampleDepartment();
    }

    private static void UpdateCallouts()
    {
        var addonMetadataService = provider.GetService<IInternalAddonMetadataService>();
        addonMetadataService.CreateCalloutDetailsEntities();
    }

    private static void AddSampleDepartment()
    {
        if (databaseContext.Departments.Any())
        {
            return;
        }

        var department = new Department
        {
            Name = "Los Santos Police Department",
            Callsign = "LSPD",
            MaxUsers = 0,
            CallsignFormat = "*L*S-NN",
            Ranks = new List<Rank>
            {
                new()
                {
                    Name = "Officer I",
                    RequiredXp = 0,
                    Permission = new Rank.Permissions
                    {
                        Kick = false,
                        SetRank = false,
                        SetDivision = false,
                    },
                },
                new()
                {
                    Name = "Officer II",
                    RequiredXp = 0,
                    Permission = new Rank.Permissions
                    {
                        Kick = false,
                        SetRank = false,
                        SetDivision = false,
                    },
                },
                new()
                {
                    Name = "Sergeant",
                    RequiredXp = 0,
                    Permission = new Rank.Permissions
                    {
                        Kick = false,
                        SetRank = false,
                        SetDivision = false,
                    },
                },
            },
        };
        databaseContext.Departments.Add(department);
        databaseContext.SaveChanges();
        logger?.Information("Created sample department.");
    }
}