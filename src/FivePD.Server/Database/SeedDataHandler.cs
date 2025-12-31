using System.Collections.Generic;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.AssistanceServiceModels;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FivePD.Server.Database;

#pragma warning disable

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
        this.SeedSampleRanks();
        this.SeedSampleDepartment();
        this.SeedVehicleConfigs();
        this.SeedLoadoutConfigs();
    }

    private void SeedSampleDepartment()
    {
        using var connection = this._connectionFactory.CreateScope().Connection;
        var departmentRepository = this._serviceProvider.GetService<DepartmentRepository>();
        if (departmentRepository.Count() > 0)
        {
            this._logger.Debug("Departments already exist, skipping department seeding.");
            return;
        }

        this._logger.Debug("Creating sample department with divisions...");

        var department = departmentRepository.Add(new Department
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
        });

        var divisionRepository = this._serviceProvider.GetService<DivisionRepository>();
        divisionRepository.Add(new Division
        {
            Name = "Patrol division",
            DepartmentId = department.Id,
        });
        divisionRepository.Add(new Division
        {
            Name = "Detective division",
            DepartmentId = department.Id,
        });
        divisionRepository.Add(new Division
        {
            Name = "SWAT division",
            DepartmentId = department.Id,
        });
        divisionRepository.Add(new Division
        {
            Name = "Traffic division",
            DepartmentId = department.Id,
        });

        this._logger.Information("Created sample department with divisions.");
    }

    private void SeedSampleRanks()
    {
        using var connection = this._connectionFactory.CreateScope().Connection;
        var rankRepository = this._serviceProvider.GetService<RankRepository>();
        if (rankRepository.Count() > 0)
        {
            this._logger.Debug("Ranks already exist, skipping rank seeding.");
            return;
        }

        this._logger.Debug("Creating sample ranks...");

        rankRepository.Add(new Rank
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
        });
        rankRepository.Add(new Rank
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
        });
        rankRepository.Add(new Rank
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
        this._logger.Information("Created sample ranks.");
    }

    private void SeedVehicleConfigs()
    {
        using var connection = this._connectionFactory.CreateScope().Connection;
        var vehicleConfigRepository = this._serviceProvider.GetService<VehicleConfigRepository>();
        if (vehicleConfigRepository.Count() > 0)
        {
            this._logger.Debug("Vehicle configs already exist, skipping vehicle config seeding.");
            return;
        }

        this._logger.Debug("Creating sample vehicle configs...");

        var ambulance = DefaultVehicleConfigs.Ambulance;
        var police = DefaultVehicleConfigs.Police;
        var towTruck = DefaultVehicleConfigs.TowTruck;
        var rideShare = DefaultVehicleConfigs.RideShare;
        var coroner = DefaultVehicleConfigs.Coroner;

        vehicleConfigRepository.Add(ambulance);
        vehicleConfigRepository.Add(police);
        vehicleConfigRepository.Add(towTruck);
        vehicleConfigRepository.Add(rideShare);
        vehicleConfigRepository.Add(coroner);

        this._logger.Information("Created sample vehicle configs.");
    }

    private void SeedLoadoutConfigs()
    {
        using var connection = this._connectionFactory.CreateScope().Connection;
        var loadoutConfigRepository = this._serviceProvider.GetService<LoadoutConfigRepository>();
        var weaponRepository = this._serviceProvider.GetService<WeaponRepository>();
        var weaponComponentRepository = this._serviceProvider.GetService<WeaponComponentRepository>();

        if (loadoutConfigRepository.Count() > 0)
        {
            this._logger.Debug("Loadout configs already exist, skipping loadout config seeding.");
            return;
        }

        this._logger.Debug("Creating sample loadout configs...");

        var pistolId = weaponRepository.Add(new Weapon { Name = "Pistol", Model = "WEAPON_PISTOL", });
        var carbineRifleId = weaponRepository.Add(new Weapon { Name = "Carbine Rifle", Model = "WEAPON_CARBINERIFLE", });
        var taserId = weaponRepository.Add(new Weapon { Name = "Taser", Model = "WEAPON_STUNGUN", });

        var suppressorId = weaponComponentRepository.Add(new WeaponComponent { Name = "Suppressor", HashKey = "COMPONENT_AT_PI_SUPP_02", });
        var scopeId = weaponComponentRepository.Add(new WeaponComponent { Name = "Scope", HashKey = "COMPONENT_AT_SCOPE_MEDIUM", });

        weaponRepository.AttachComponent(pistolId, suppressorId);
        weaponRepository.AttachComponent(carbineRifleId, scopeId);

        var patrolLoadoutId = loadoutConfigRepository.Add(new LoadoutConfig { Name = "Patrol loadout" });
        var swatLoadoutId = loadoutConfigRepository.Add(new LoadoutConfig { Name = "SWAT loadout" });

        loadoutConfigRepository.AddWeapon(patrolLoadoutId, pistolId);
        loadoutConfigRepository.AddWeapon(patrolLoadoutId, taserId);

        loadoutConfigRepository.AddWeapon(swatLoadoutId, carbineRifleId);
        loadoutConfigRepository.AddWeapon(swatLoadoutId, taserId);

        this._logger.Information("Created sample loadout configs.");
    }
}