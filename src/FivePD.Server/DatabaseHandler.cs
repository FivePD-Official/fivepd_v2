using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Server.Database;
using FivePD.Server.Exceptions;
using FivePD.Server.Interfaces.Addons;
using FivePD.Server.Models;
using FivePD.Server.Models.AssistanceServiceModels;
using FivePD.Server.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FivePD.Server;

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
        SeedSampleDepartment();
        SeedVehicleConfigs();
    }

    private static void SeedCallouts()
    {
        var addonMetadataService = _provider.GetService<IInternalAddonMetadataService>();
        addonMetadataService.CreateCalloutDetailsEntities();
    }

    private static void SeedSampleDepartment()
    {
        if (_databaseContext.Departments.Any())
        {
            return;
        }

        var department = new Department
        {
            Name = "Los Santos Police Department",
            Callsign = "LSPD",
            MaxUsers = 0,
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
                        DepartmentManager = false,
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
                        DepartmentManager = false,
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
                        DepartmentManager = false,
                    },
                },
            },
        };
        _databaseContext.Departments.Add(department);
        _databaseContext.SaveChanges();
        _logger?.Information("Created sample department.");
    }

    private static void SeedVehicleConfigs()
    {
        if (_databaseContext.VehicleConfigs.Any())
        {
            return;
        }

        var ambulance = DefaultVehicleConfigs.Ambulance;
        var police = DefaultVehicleConfigs.Police;
        var towTruck = VehicleConfigOptionsConverter.ConvertOptionsToDictionary(DefaultVehicleConfigs.TowTruck);

        var towTrucks = new List<VehicleConfig<Dictionary<string, object>>>() { ambulance, police, towTruck };

        _databaseContext.VehicleConfigs.AddRange(towTrucks);
        _databaseContext.SaveChanges();
        _logger?.Information("Created sample vehicle configs.");
    }
}