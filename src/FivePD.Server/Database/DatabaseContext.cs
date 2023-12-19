using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common.ConfigModels;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.LooseData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;

namespace FivePD.Server.Database;

/// <summary>
/// Provides access to the underlying database implementation.
/// Most modifications to the database should be handled through
/// abstractions in other areas of the codebase. Addon developers
/// should rarely, if ever, have to access this class.
/// </summary>
public class DatabaseContext : DbContext
{
    private readonly IConfigService _configService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
    /// </summary>
    /// <param name="options">The <see cref="DbContextOptions"/> to use.</param>
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
    /// </summary>
    /// <param name="options">asd.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfigService configService)
        : base(options)
    {
        this._configService = configService;
    }

    /// <summary>
    /// Gets or sets the users table.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Gets or sets departments table.
    /// </summary>
    public DbSet<Department> Departments { get; set; }

    /// <summary>
    /// Gets or sets departments area coordinates table.
    /// </summary>
    public DbSet<DepartmentAreaCoordinate> DepartmentAreaCoordinates { get; set; }

    /// <summary>
    /// Gets or sets divisions table.
    /// </summary>
    public DbSet<Division> Divisions { get; set; }

    /// <summary>
    /// Gets or sets ranks table.
    /// </summary>
    public DbSet<Rank> Ranks { get; set; }

    /// <summary>
    /// Gets or sets the table containing the dynamic data sets.
    /// </summary>
    public DbSet<LooseDataSet> LooseDataSets { get; set; }

    /// <summary>
    /// Gets or sets the table containing the dynamic entities.
    /// </summary>
    public DbSet<LooseDataEntity> LooseDataEntities { get; set; }

    /// <summary>
    /// Gets or sets the table containing the attributes of the dynamic data sets.
    /// </summary>
    public DbSet<LooseDataAttribute> LooseDataAttributes { get; set; }

    /// <summary>
    /// Gets or sets the table containing the values of dynamic entities.
    /// </summary>
    public DbSet<LooseDataValue> LooseDataValues { get; set; }

    /// <summary>
    /// Gets or sets the table containing the incident histories.
    /// </summary>
    public DbSet<IncidentHistory> IncidentHistories { get; set; }

    /// <summary>
    /// Gets or sets the table containing the assigned users of an incident history.
    /// </summary>
    public DbSet<IncidentHistoryUser> IncidentHistoryUsers { get; set; }

    /// <summary>
    /// Gets or sets the table containing the ped histories.
    /// </summary>
    public DbSet<PedHistory> PedHistories { get; set; }

    /// <summary>
    /// Gets or sets the table containing the ped licenses.
    /// </summary>
    public DbSet<PedLicense> PedLicenses { get; set; }

    /// <summary>
    /// Gets or sets the table containing the messages.
    /// </summary>
    public DbSet<Message> Messages { get; set; }

    /// <summary>
    /// Gets or sets the table containing the callout details.
    /// </summary>
    public DbSet<CalloutDetails> CalloutDetails { get; set; }

    /// <summary>
    /// Gets or sets the table containing the callout details departments.
    /// </summary>
    public DbSet<CalloutDetailsDepartment> CalloutDetailsDepartments { get; set; }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = this._configService?.GeneralConfig?.Database;
        if (config is not null)
        {
            optionsBuilder.UseMySql(config.ToString(), options =>
            {
                var serverVersion = ServerVersion.AutoDetect(config.ToString());
                options.ServerVersion(serverVersion.Version, config.IsMariaDb() ? ServerType.MariaDb : ServerType.MySql);
            });
        }
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(type =>
        {
            type.HasMany(department => department.Users)
                .WithOne(user => user.Department)
                .OnDelete(DeleteBehavior.Cascade);

            type.HasMany(department => department.Divisions)
                .WithOne(division => division.Department)
                .OnDelete(DeleteBehavior.Cascade);

            type.HasMany(department => department.Ranks)
                .WithOne(user => user.Department)
                .OnDelete(DeleteBehavior.Cascade);

            type.HasOne(department => department.DefaultRank);

            type.Property(t => t.CallsignFormat)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    v => JsonConvert.DeserializeObject<List<CallsignFormatItem>>(
                        v,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        });

        modelBuilder.Entity<CalloutDetailsDepartment>(type =>
        {
            type.HasKey(bc => new { bc.CalloutDetailsId, bc.DepartmentId });

            type.HasOne(bc => bc.CalloutDetails)
                .WithMany(b => b.CalloutDetailsDepartments)
                .HasForeignKey(bc => bc.CalloutDetailsId);

            type.HasOne(bc => bc.Department)
                .WithMany(c => c.CalloutDetailsDepartments)
                .HasForeignKey(bc => bc.DepartmentId);
        });

        modelBuilder.Entity<User>(type =>
        {
            type.HasOne(user => user.Rank);

            type.HasOne(user => user.Division);

            type.HasMany(user => user.Messages)
                .WithOne(message => message.User)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Rank>()
            .OwnsOne(rank => rank.Permission)
            .WithOwner();

        modelBuilder.Entity<LooseDataSet>(type =>
        {
            type.HasMany(t => t.Entities);
            type.HasMany(t => t.Attributes);
        });

        modelBuilder.Entity<LooseDataAttribute>(type =>
        {
            type.Property(t => t.Options)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    v => JsonConvert.DeserializeObject<LooseDataAttribute.AttributeOptions>(
                        v,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        });

        modelBuilder.Entity<LooseDataEntity>(type =>
        {
            type.HasOne(t => t.Set);
            type.HasMany(t => t.Values)
                .WithOne(x => x.Entity)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LooseDataValue>(type =>
        {
            type.HasOne(t => t.Attribute);
            type.HasOne(t => t.Entity);
        });

        modelBuilder.Entity<IncidentHistory>()
            .HasMany(incidentHistory => incidentHistory.Users)
            .WithOne(user => user.IncidentHistory)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PedHistory>(type =>
        {
            type.Property(t => t.Items)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    v => JsonConvert.DeserializeObject<List<Item>>(
                        v,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            type.Property(t => t.Warrants)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    v => JsonConvert.DeserializeObject<List<string>>(
                        v,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            type.HasMany(pedHistory => pedHistory.Licenses)
                .WithOne(license => license.PedHistory)
                .OnDelete(DeleteBehavior.Cascade);

            type.Property(t => t.Drugs)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    v => JsonConvert.DeserializeObject<List<Drug>>(
                        v,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        });

        modelBuilder.Entity<VehicleConfig>(type =>
        {
            type.Property(t => t.Options)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    v => JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        v,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            type.Property(t => t.Extras)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    v => JsonConvert.DeserializeObject<List<int>>(
                        v,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        });
    }
}