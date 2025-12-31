using System;
using System.Collections.Generic;
using System.Linq;
using FivePD.Server.Database;
using FivePD.Server.Exceptions;
using FivePD.Server.Interfaces.Addons;
using FivePD.Server.Models;
using FivePD.Server.Models.AssistanceServiceModels;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Models.LoadoutModels.JoinTables;
using FivePD.Server.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FivePD.Server;

#pragma warning disable

/// <summary>
/// Handles database operations and ensures the correct setup and migration of the database.
/// </summary>
internal static class DatabaseHandler
{
    private static ServiceProvider _provider;
    private static ILogger _logger;
    private static DatabaseContext _databaseContext;

    /// <summary>
    /// Sets the parameters of the current instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider to be used.</param>
    /// <param name="logger">The logger to be used.</param>
    /// <exception cref="DatabaseUsageException">Thrown when no database context is available for usage.</exception>
    public static void SetParameters(ServiceProvider serviceProvider, ILogger logger)
    {
        _provider = serviceProvider;
        _logger = logger;
        _databaseContext = serviceProvider.GetService<DatabaseContext>();

        if (_databaseContext is null)
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
            _databaseContext.Database.Migrate();
            if (!_databaseContext.Database.CanConnect())
            {
                throw new DatabaseUsageException("Database connection failed");
            }

            _logger?.Information("The database is up to date");
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
        if (_databaseContext.Departments.Any())
        {
            _logger?.Debug("Departments already exist, skipping department seeding.");
            return;
        }

        _logger?.Debug("Creating sample department with divisions...");

        var department = new Department
        {
            Name = "Los Santos Police Department",
            Callsign = "LSPD",
            MaxUsers = 0,
            UseGlobalRanks = true,
            CallsignFormat = new List<CallsignFormatItem>
            {
                new()
                {
                    Type = CallsignFormatItemType.FixedCharacter,
                    Value = "L",
                },
                new()
                {
                    Type = CallsignFormatItemType.FixedCharacter,
                    Value = "S",
                },
                new()
                {
                    Type = CallsignFormatItemType.Dash,
                },
                new()
                {
                    Type = CallsignFormatItemType.RandomNumber,
                },
            },
            Divisions = new List<Division>
            {
                new() { Name = "Patrol division" },
                new() { Name = "Detective division" },
                new() { Name = "SWAT division" },
                new() { Name = "Traffic division" },
            },
        };
        _databaseContext.Departments.Add(department);
        _databaseContext.SaveChanges();
        _logger?.Information("Created sample department with divisions.");
    }

    private static void SeedSampleRanks()
    {
        if (_databaseContext.Ranks.Any())
        {
            _logger?.Debug("Ranks already exist, skipping rank seeding.");
            return;
        }

        _logger?.Debug("Creating sample ranks...");

        _databaseContext.Ranks.AddRange(
            new()
            {
                Name = "Officer I",
                RequiredXp = 0,
                Global = true,
                Permission = new Rank.Permissions
                {
                    Kick = false,
                    SetRank = false,
                    SetDivision = false,
                    DepartmentManager = false,
                },
            },
            new()
            {
                Name = "Officer II",
                RequiredXp = 0,
                Global = true,
                Permission = new Rank.Permissions
                {
                    Kick = false,
                    SetRank = false,
                    SetDivision = false,
                    DepartmentManager = false,
                },
            },
            new()
            {
                Name = "Sergeant",
                RequiredXp = 0,
                Global = true,
                Permission = new Rank.Permissions
                {
                    Kick = false,
                    SetRank = false,
                    SetDivision = false,
                    DepartmentManager = false,
                },
            });
        _databaseContext.SaveChanges();
        _logger?.Information("Created sample ranks.");
    }

    private static void SeedVehicleConfigs()
    {
        if (_databaseContext.VehicleConfigs.Any())
        {
            _logger?.Debug("Vehicle configs already exist, skipping vehicle config seeding.");
            return;
        }

        _logger?.Debug("Creating sample vehicle configs...");

        var ambulance = DefaultVehicleConfigs.Ambulance;
        var police = DefaultVehicleConfigs.Police;
        var towTruck = VehicleConfigOptionsConverter.ConvertOptionsToDictionary(DefaultVehicleConfigs.TowTruck);
        var rideShare = DefaultVehicleConfigs.RideShare;
        var coroner = DefaultVehicleConfigs.Coroner;

        var vehicleConfigs = new List<VehicleConfig<Dictionary<string, object>>>() { ambulance, police, towTruck, rideShare, coroner };

        _databaseContext.VehicleConfigs.AddRange(vehicleConfigs);
        _databaseContext.SaveChanges();
        _logger?.Information("Created sample vehicle configs.");
    }

    private static void SeedLoadoutConfigs()
    {
        if (_databaseContext.LoadoutConfigs.Any())
        {
            _logger?.Debug("Loadout configs already exist, skipping loadout config seeding.");
            return;
        }

        _logger?.Debug("Creating sample loadout configs...");

        var pistol = new Weapon
        {
            Name = "Pistol",
            Model = "WEAPON_PISTOL",
        };

        var rifle = new Weapon
        {
            Name = "Carbine Rifle",
            Model = "WEAPON_CARBINERIFLE",
        };

        var taser = new Weapon
        {
            Name = "Taser",
            Model = "WEAPON_STUNGUN",
        };

        var pistolSuppressor = new WeaponComponent
        {
            Name = "Suppressor",
            HashKey = "COMPONENT_AT_PI_SUPP_02",
        };

        var rifleScope = new WeaponComponent
        {
            Name = "Scope",
            HashKey = "COMPONENT_AT_SCOPE_MEDIUM",
        };

        var pistolSuppressorJoin = new WeaponWeaponComponent { Weapon = pistol, WeaponComponent = pistolSuppressor };
        var rifleScopeJoin = new WeaponWeaponComponent { Weapon = rifle, WeaponComponent = rifleScope };

        pistol.WeaponWeaponComponents.Add(pistolSuppressorJoin);
        rifle.WeaponWeaponComponents.Add(rifleScopeJoin);

        var patrolLoadout = new LoadoutConfig
        {
            Name = "Patrol Loadout",
        };
        // patrolLoadout.AddWeapon(pistol);
        // patrolLoadout.AddWeapon(taser);

        var swatLoadout = new LoadoutConfig
        {
            Name = "SWAT Loadout",
        };
        // swatLoadout.AddWeapon(rifle);
        // swatLoadout.AddWeapon(pistol);

        var loadouts = new List<LoadoutConfig> { patrolLoadout, swatLoadout };
        var weapons = new List<Weapon> { pistol, rifle, taser };
        var components = new List<WeaponComponent> { pistolSuppressor, rifleScope };

        _databaseContext.Weapons.AddRange(weapons);
        _databaseContext.WeaponComponents.AddRange(components);
        _databaseContext.LoadoutConfigs.AddRange(loadouts);
        _databaseContext.SaveChanges();
        _logger?.Information("Created sample loadout configs.");
    }
}