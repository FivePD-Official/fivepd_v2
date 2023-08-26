using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.LooseData;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = this._configService?.GeneralConfig?.Database;
        if (config != null)
        {
            optionsBuilder.UseMySql(config.ToString());
        }
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>()
            .HasMany(department => department.Users)
            .WithOne(user => user.Department)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Department>()
            .HasMany(department => department.Divisions)
            .WithOne(division => division.Department)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Department>()
            .HasMany(department => department.Ranks)
            .WithOne(user => user.Department)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Department>()
            .HasOne(department => department.DefaultRank);

        modelBuilder.Entity<User>()
            .HasOne(user => user.Division);

        modelBuilder.Entity<User>()
            .HasOne(user => user.Rank);

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
                    v => JsonConvert.DeserializeObject<LooseDataAttribute.AttributeOptions>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        });

        modelBuilder.Entity<LooseDataEntity>(type =>
        {
            type.HasOne(t => t.Set);
            type.HasMany(t => t.Values).WithOne(x => x.Entity).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LooseDataValue>(type =>
        {
            type.HasOne(t => t.Attribute);
            type.HasOne(t => t.Entity);
        });
    }
}