using System.Collections.Generic;
using Dapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FivePD.Server.Database;

/// <summary>
/// Handles database seeding operations for initial data setup.
/// </summary>
internal class SeedDataHandler
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly IDapperDatabaseConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedDataHandler"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="logger">The logger for logging operations.</param>
    /// <param name="connectionFactory">The database connection factory for creating database connections.</param>
    public SeedDataHandler(ServiceProvider serviceProvider, ILogger logger, IDapperDatabaseConnectionFactory connectionFactory)
    {
        this._serviceProvider = serviceProvider;
        this._logger = logger;
        this._connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Seeds sample data into the database.
    /// </summary>
    public void SeedData()
    {
        this.SeedCallouts();
        this.SeedSampleRanks();
        this.SeedSampleDepartment();
        this.SeedVehicleConfigs();
        this.SeedLoadoutConfigs();
    }

// TODO: might want to move to a service
    private void SeedCallouts()
    {
        this._logger.Debug("Seeding callout detail entities from addons...");
#pragma warning disable
        // var addonMetadataService = this._serviceProvider.GetService<IInternalAddonMetadataService>();
        // addonMetadataService.CreateCalloutDetailsEntities();
#pragma warning restore
        this._logger.Debug("Seeding of callout detail entities completed.");
    }

    private void SeedSampleDepartment()
    {
        using var connection = this._connectionFactory.CreateScope().Connection;

        var existingCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM fivepd_departments");
        if (existingCount > 0)
        {
            this._logger.Debug("Departments already exist, skipping department seeding.");
            return;
        }

        this._logger.Debug("Creating sample department with divisions...");

        var departmentId = connection.ExecuteScalar<int>(
            @"
            INSERT INTO fivepd_departments (Name, Callsign, MaxUsers, UseGlobalRanks, CallsignFormat)
            VALUES (@Name, @Callsign, @MaxUsers, @UseGlobalRanks, @CallsignFormat);
            SELECT LAST_INSERT_ID();",
            new
            {
                Name = "Los Santos Police Department",
                Callsign = "LSPD",
                MaxUsers = 0,
                UseGlobalRanks = true,
                CallsignFormat = "[{\"Type\":0,\"Value\":\"L\"},{\"Type\":0,\"Value\":\"S\"},{\"Type\":2},{\"Type\":3}]",
            });

        var divisions = new[]
        {
            new { Name = "Patrol division", DepartmentId = departmentId },
            new { Name = "Detective division", DepartmentId = departmentId },
            new { Name = "SWAT division", DepartmentId = departmentId },
            new { Name = "Traffic division", DepartmentId = departmentId },
        };

        connection.Execute(
            @"
            INSERT INTO fivepd_divisions (Name, DepartmentId)
            VALUES (@Name, @DepartmentId)",
            divisions);

        this._logger.Information("Created sample department with divisions.");
    }

    private void SeedSampleRanks()
    {
        using var connection = this._connectionFactory.CreateScope().Connection;

        var existingCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM fivepd_ranks");
        if (existingCount > 0)
        {
            this._logger.Debug("Ranks already exist, skipping rank seeding.");
            return;
        }

        this._logger.Debug("Creating sample ranks...");

        var ranks = new[]
        {
            new
            {
                Name = "Officer I",
                RequiredXp = 0,
                Global = true,
                Permission = "{\"Kick\":false,\"SetRank\":false,\"SetDivision\":false,\"DepartmentManager\":false}",
            },
            new
            {
                Name = "Officer II",
                RequiredXp = 0,
                Global = true,
                Permission = "{\"Kick\":false,\"SetRank\":false,\"SetDivision\":false,\"DepartmentManager\":false}",
            },
            new
            {
                Name = "Sergeant",
                RequiredXp = 0,
                Global = true,
                Permission = "{\"Kick\":false,\"SetRank\":false,\"SetDivision\":false,\"DepartmentManager\":false}",
            },
        };

        connection.Execute(
            @"
            INSERT INTO fivepd_ranks (Name, RequiredXp, Global, Permission)
            VALUES (@Name, @RequiredXp, @Global, @Permission)",
            ranks);

        this._logger.Information("Created sample ranks.");
    }

    private void SeedVehicleConfigs()
    {
        using var connection = this._connectionFactory.CreateScope().Connection;

        var existingCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM fivepd_vehicle_configs");
        if (existingCount > 0)
        {
            this._logger.Debug("Vehicle configs already exist, skipping vehicle config seeding.");
            return;
        }

        this._logger.Debug("Creating sample vehicle configs...");

        var vehicleConfigs = new[]
        {
            new
            {
                Name = "Ambulance",
                Type = VehicleConfigType.Ambulance,
                Model = "ambulance",
                Options = "{\"siren\":true,\"lights\":true,\"medical\":true}",
                Extras = "[0,1,2,3]",
            },
            new
            {
                Name = "Police Cruiser",
                Type = VehicleConfigType.Police,
                Model = "police",
                Options = "{\"siren\":true,\"lights\":true,\"weapons\":true}",
                Extras = "[0,1,2]",
            },
            new
            {
                Name = "Tow Truck",
                Type = VehicleConfigType.TowTruck,
                Model = "towtruck",
                Options = "{\"tow\":true}",
                Extras = "[0]",
            },
            new
            {
                Name = "Rideshare",
                Type = VehicleConfigType.RideShare,
                Model = "taxi",
                Options = "{\"meter\":true}",
                Extras = "[0]",
            },
            new
            {
                Name = "Coroner",
                Type = VehicleConfigType.Coroner,
                Model = "hearse",
                Options = "{\"medical\":true}",
                Extras = "[0]",
            },
        };

        connection.Execute(
            @"
            INSERT INTO fivepd_vehicle_configs (Name, Type, Model, Options, Extras)
            VALUES (@Name, @Type, @Model, @Options, @Extras)",
            vehicleConfigs);

        this._logger.Information("Created sample vehicle configs.");
    }

    private void SeedLoadoutConfigs()
    {
        using var connection = this._connectionFactory.CreateScope().Connection;

        var existingCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM fivepd_loadout_configs");
        if (existingCount > 0)
        {
            this._logger.Debug("Loadout configs already exist, skipping loadout config seeding.");
            return;
        }

        this._logger.Debug("Creating sample loadout configs...");

        var weapons = new[]
        {
            new { Name = "Pistol", Model = "WEAPON_PISTOL" },
            new { Name = "Carbine Rifle", Model = "WEAPON_CARBINERIFLE" },
            new { Name = "Taser", Model = "WEAPON_STUNGUN" },
        };

        var weaponIds = new Dictionary<string, int>();
        foreach (var weapon in weapons)
        {
            var id = connection.ExecuteScalar<int>(
                @"
                INSERT INTO fivepd_weapons (Name, Model)
                VALUES (@Name, @Model);
                SELECT LAST_INSERT_ID();",
                weapon);
            weaponIds[weapon.Name] = id;
        }

        var components = new[]
        {
            new { Name = "Suppressor", HashKey = "COMPONENT_AT_PI_SUPP_02" },
            new { Name = "Scope", HashKey = "COMPONENT_AT_SCOPE_MEDIUM" },
        };

        var componentIds = new Dictionary<string, int>();
        foreach (var component in components)
        {
            var id = connection.ExecuteScalar<int>(
                @"
                INSERT INTO fivepd_weapon_components (Name, HashKey)
                VALUES (@Name, @HashKey);
                SELECT LAST_INSERT_ID();",
                component);
            componentIds[component.Name] = id;
        }

        var patrolLoadoutId = connection.ExecuteScalar<int>(@"
            INSERT INTO fivepd_loadout_configs (Name)
            VALUES ('Patrol Loadout');
            SELECT LAST_INSERT_ID();");

        var swatLoadoutId = connection.ExecuteScalar<int>(@"
            INSERT INTO fivepd_loadout_configs (Name)
            VALUES ('SWAT Loadout');
            SELECT LAST_INSERT_ID();");

        connection.Execute(
            @"
            INSERT INTO fivepd_loadout_weapon (LoadoutConfigId, WeaponId)
            VALUES (@LoadoutId, @WeaponId)",
            new[]
            {
                new { LoadoutId = patrolLoadoutId, WeaponId = weaponIds["Pistol"] },
                new { LoadoutId = patrolLoadoutId, WeaponId = weaponIds["Taser"] },
                new { LoadoutId = swatLoadoutId, WeaponId = weaponIds["Carbine Rifle"] },
                new { LoadoutId = swatLoadoutId, WeaponId = weaponIds["Pistol"] },
            });

        connection.Execute(
            @"
            INSERT INTO fivepd_weapon_weaponComponent (WeaponId, WeaponComponentId)
            VALUES (@WeaponId, @ComponentId)",
            new[]
            {
                new { WeaponId = weaponIds["Pistol"], ComponentId = componentIds["Suppressor"] },
                new { WeaponId = weaponIds["Carbine Rifle"], ComponentId = componentIds["Scope"] },
            });

        this._logger.Information("Created sample loadout configs.");
    }
}