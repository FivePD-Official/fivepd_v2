using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Exceptions;
using FivePD.Server.Interfaces.Addons;
using FivePD.Server.Models;
using FivePD.Server.Models.AssistanceServiceModels;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Models.LoadoutModels.JoinTables;
using FivePD.Server.Utilities;
using Serilog;

namespace FivePD.Server;

/// <summary>
/// Handles database operations and ensures the correct setup and migration of the database using Dapper.
/// Replaces the EF Core DatabaseHandler.
/// </summary>
internal static class DapperDatabaseHandler
{
    private static ServiceProvider _provider;
    private static ILogger _logger;
    private static IDatabaseContextFactory _dbContextFactory;
    private static DatabaseMigrationManager _migrationManager;

    /// <summary>
    /// Sets the parameters of the current instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider to be used.</param>
    /// <param name="logger">The logger to be used.</param>
    public static void SetParameters(ServiceProvider serviceProvider, ILogger logger)
    {
        _provider = serviceProvider;
        _logger = logger;
        _dbContextFactory = serviceProvider.GetService<IDatabaseContextFactory>();

        if (_dbContextFactory is null)
        {
            throw new DatabaseUsageException("No database context factory is available for usage.");
        }

        _migrationManager = new DatabaseMigrationManager(_dbContextFactory, logger, serviceProvider.GetService<IConfigService>());
    }

    /// <summary>
    /// Ensures that the database is in the correct state.
    /// </summary>
    public static void EnsureCorrectState()
    {
        try
        {
            // Apply migrations automatically
            _migrationManager.EnsureDatabaseSchema();

            // Verify connection
            using var scope = _dbContextFactory.CreateScope();
            if (scope.Connection.State != System.Data.ConnectionState.Open)
            {
                scope.Connection.Open();
            }

            _logger?.Information("Database is ready and migrations are up to date.");
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException(
                "The configured database is invalid or offline. Verify the connection settings in the /Config/general file and try again.", e);
        }
    }

    /// <summary>
    /// Lists all applied migrations.
    /// </summary>
    public static void ListMigrations()
    {
        _migrationManager.ListAppliedMigrations();
    }

    /// <summary>
    /// Reverts a specific migration.
    /// </summary>
    public static void RevertMigration(string migrationId)
    {
        _migrationManager.RevertMigration(migrationId);
    }

    /// <summary>
    /// Updates the database with a sample department if there are none
    /// and updates the callouts.
    /// </summary>
    public static void SeedData()
    {
        SeedCallouts();
        SeedSampleRanks();
        SeedSampleDepartment();
        SeedVehicleConfigs();
        SeedLoadoutConfigs();
    }

    private static void SeedCallouts()
    {
        _logger?.Debug("Seeding callout detail entities from addons...");
        var addonMetadataService = _provider.GetService<IInternalAddonMetadataService>();
        addonMetadataService.CreateCalloutDetailsEntities();
        _logger?.Debug("Seeding of callout detail entities completed.");
    }

    private static void SeedSampleDepartment()
    {
        using var scope = _dbContextFactory.CreateScope();
        var connection = scope.Connection;

        var existingCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM fivepd_departments");
        if (existingCount > 0)
        {
            _logger?.Debug("Departments already exist, skipping department seeding.");
            return;
        }

        _logger?.Debug("Creating sample department with divisions...");

        // Insert department
        var departmentId = connection.ExecuteScalar<int>(@"
            INSERT INTO fivepd_departments (Name, Callsign, MaxUsers, UseGlobalRanks, CallsignFormat)
            VALUES (@Name, @Callsign, @MaxUsers, @UseGlobalRanks, @CallsignFormat);
            SELECT LAST_INSERT_ID();", new
        {
            Name = "Los Santos Police Department",
            Callsign = "LSPD",
            MaxUsers = 0,
            UseGlobalRanks = true,
            CallsignFormat = "[{\"Type\":0,\"Value\":\"L\"},{\"Type\":0,\"Value\":\"S\"},{\"Type\":2},{\"Type\":3}]"
        });

        // Insert divisions
        var divisions = new[]
        {
            new { Name = "Patrol division", DepartmentId = departmentId },
            new { Name = "Detective division", DepartmentId = departmentId },
            new { Name = "SWAT division", DepartmentId = departmentId },
            new { Name = "Traffic division", DepartmentId = departmentId }
        };

        connection.Execute(@"
            INSERT INTO fivepd_divisions (Name, DepartmentId)
            VALUES (@Name, @DepartmentId)", divisions);

        _logger?.Information("Created sample department with divisions.");
    }

    private static void SeedSampleRanks()
    {
        using var scope = _dbContextFactory.CreateScope();
        var connection = scope.Connection;

        var existingCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM fivepd_ranks");
        if (existingCount > 0)
        {
            _logger?.Debug("Ranks already exist, skipping rank seeding.");
            return;
        }

        _logger?.Debug("Creating sample ranks...");

        var ranks = new[]
        {
            new
            {
                Name = "Officer I",
                RequiredXp = 0,
                Global = true,
                Permission = "{\"Kick\":false,\"SetRank\":false,\"SetDivision\":false,\"DepartmentManager\":false}"
            },
            new
            {
                Name = "Officer II",
                RequiredXp = 0,
                Global = true,
                Permission = "{\"Kick\":false,\"SetRank\":false,\"SetDivision\":false,\"DepartmentManager\":false}"
            },
            new
            {
                Name = "Sergeant",
                RequiredXp = 0,
                Global = true,
                Permission = "{\"Kick\":false,\"SetRank\":false,\"SetDivision\":false,\"DepartmentManager\":false}"
            }
        };

        connection.Execute(@"
            INSERT INTO fivepd_ranks (Name, RequiredXp, Global, Permission)
            VALUES (@Name, @RequiredXp, @Global, @Permission)", ranks);

        _logger?.Information("Created sample ranks.");
    }

    private static void SeedVehicleConfigs()
    {
        using var scope = _dbContextFactory.CreateScope();
        var connection = scope.Connection;

        var existingCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM fivepd_vehicle_configs");
        if (existingCount > 0)
        {
            _logger?.Debug("Vehicle configs already exist, skipping vehicle config seeding.");
            return;
        }

        _logger?.Debug("Creating sample vehicle configs...");

        var vehicleConfigs = new[]
        {
            new
            {
                Name = "Ambulance",
                Model = "ambulance",
                Options = "{\"siren\":true,\"lights\":true,\"medical\":true}",
                Extras = "[0,1,2,3]"
            },
            new
            {
                Name = "Police Cruiser",
                Model = "police",
                Options = "{\"siren\":true,\"lights\":true,\"weapons\":true}",
                Extras = "[0,1,2]"
            },
            new
            {
                Name = "Tow Truck",
                Model = "towtruck",
                Options = "{\"tow\":true}",
                Extras = "[0]"
            },
            new
            {
                Name = "Rideshare",
                Model = "taxi",
                Options = "{\"meter\":true}",
                Extras = "[0]"
            },
            new
            {
                Name = "Coroner",
                Model = "hearse",
                Options = "{\"medical\":true}",
                Extras = "[0]"
            }
        };

        connection.Execute(@"
            INSERT INTO fivepd_vehicle_configs (Name, Model, Options, Extras)
            VALUES (@Name, @Model, @Options, @Extras)", vehicleConfigs);

        _logger?.Information("Created sample vehicle configs.");
    }

    private static void SeedLoadoutConfigs()
    {
        using var scope = _dbContextFactory.CreateScope();
        var connection = scope.Connection;

        var existingCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM fivepd_loadout_configs");
        if (existingCount > 0)
        {
            _logger?.Debug("Loadout configs already exist, skipping loadout config seeding.");
            return;
        }

        _logger?.Debug("Creating sample loadout configs...");

        // Insert weapons
        var weapons = new[]
        {
            new { Name = "Pistol", Model = "WEAPON_PISTOL" },
            new { Name = "Carbine Rifle", Model = "WEAPON_CARBINERIFLE" },
            new { Name = "Taser", Model = "WEAPON_STUNGUN" }
        };

        var weaponIds = new Dictionary<string, int>();
        foreach (var weapon in weapons)
        {
            var id = connection.ExecuteScalar<int>(@"
                INSERT INTO fivepd_weapons (Name, Model)
                VALUES (@Name, @Model);
                SELECT LAST_INSERT_ID();", weapon);
            weaponIds[weapon.Name] = id;
        }

        // Insert weapon components
        var components = new[]
        {
            new { Name = "Suppressor", HashKey = "COMPONENT_AT_PI_SUPP_02" },
            new { Name = "Scope", HashKey = "COMPONENT_AT_SCOPE_MEDIUM" }
        };

        var componentIds = new Dictionary<string, int>();
        foreach (var component in components)
        {
            var id = connection.ExecuteScalar<int>(@"
                INSERT INTO fivepd_weapon_components (Name, HashKey)
                VALUES (@Name, @HashKey);
                SELECT LAST_INSERT_ID();", component);
            componentIds[component.Name] = id;
        }

        // Insert loadout configs
        var patrolLoadoutId = connection.ExecuteScalar<int>(@"
            INSERT INTO fivepd_loadout_configs (Name)
            VALUES ('Patrol Loadout');
            SELECT LAST_INSERT_ID();");

        var swatLoadoutId = connection.ExecuteScalar<int>(@"
            INSERT INTO fivepd_loadout_configs (Name)
            VALUES ('SWAT Loadout');
            SELECT LAST_INSERT_ID();");

        // Insert loadout-weapon relationships
        connection.Execute(@"
            INSERT INTO fivepd_loadout_weapons (LoadoutConfigId, WeaponId)
            VALUES (@LoadoutId, @WeaponId)", new[]
        {
            new { LoadoutId = patrolLoadoutId, WeaponId = weaponIds["Pistol"] },
            new { LoadoutId = patrolLoadoutId, WeaponId = weaponIds["Taser"] },
            new { LoadoutId = swatLoadoutId, WeaponId = weaponIds["Carbine Rifle"] },
            new { LoadoutId = swatLoadoutId, WeaponId = weaponIds["Pistol"] }
        });

        // Insert weapon-component relationships
        connection.Execute(@"
            INSERT INTO fivepd_weapon_weapon_components (WeaponId, WeaponComponentId)
            VALUES (@WeaponId, @ComponentId)", new[]
        {
            new { WeaponId = weaponIds["Pistol"], ComponentId = componentIds["Suppressor"] },
            new { WeaponId = weaponIds["Carbine Rifle"], ComponentId = componentIds["Scope"] }
        });

        _logger?.Information("Created sample loadout configs.");
    }
}