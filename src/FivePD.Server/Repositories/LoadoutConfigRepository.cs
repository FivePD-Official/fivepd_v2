using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Models.LoadoutModels.JoinTables;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for LoadoutConfig CRUD operations.
/// </summary>
public class LoadoutConfigRepository : BaseRepository<LoadoutConfig, LoadoutConfigUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoadoutConfigRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public LoadoutConfigRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.LoadoutConfigs)
    {
    }

    /// <inheritdoc/>
    public override LoadoutConfig Add(LoadoutConfig loadoutConfig)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new { loadoutConfig.Name, })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        loadoutConfig.Id = id;
        return loadoutConfig;
    }

    /// <inheritdoc />
    public override void Update(LoadoutConfig loadoutConfig, Action<LoadoutConfigUpdate> updateAction)
    {
        var update = new LoadoutConfigUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Name), update.Name);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, loadoutConfig.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }

    /// <summary>
    /// Adds a weapon to a loadout config.
    /// </summary>
    /// <param name="loadoutConfig">The loadout config.</param>
    /// <param name="weapon">The weapon.</param>
    public void AddWeapon(LoadoutConfig loadoutConfig, Weapon weapon)
    {
        var builderResult = SqlBuilder
            .Into(TableNames.LoadoutWeapon)
            .Insert(new
            {
                LoadoutConfigId = loadoutConfig.Id,
                WeaponId = weapon.Id,
            })
            .OnDuplicateKeyUpdate("WeaponId", "LoadoutConfigId")
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Removes a weapon from a loadout config.
    /// </summary>
    /// <param name="loadoutConfig">The loadout config.</param>
    /// <param name="weapon">The weapon.</param>
    public void RemoveWeapon(LoadoutConfig loadoutConfig, Weapon weapon)
    {
        var builderResult = SqlBuilder
            .From(TableNames.LoadoutWeapon)
            .Where("LoadoutConfigId = @LoadoutConfigId", new { LoadoutConfigId = loadoutConfig.Id })
            .Where("WeaponId = @WeaponId", new { WeaponId = weapon.Id })
            .Delete()
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets all weapons in a loadout config.
    /// </summary>
    /// <param name="loadoutConfig">The loadout config.</param>
    /// <returns>List of weapons.</returns>
    public IEnumerable<Weapon> GetWeapons(LoadoutConfig loadoutConfig)
    {
        var builderResult = SqlBuilder
            .From($"{TableNames.Weapons} w")
            .Select("w.*")
            .InnerJoin(TableNames.LoadoutWeapon, "lw", "w.Id = lw.WeaponId")
            .Where("lw.LoadoutConfigId = @LoadoutConfigId", new { LoadoutConfigId = loadoutConfig.Id })
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<Weapon>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Checks if a weapon is in a loadout config.
    /// </summary>
    /// <param name="loadoutConfig">The loadout config ID.</param>
    /// <param name="weaponId">The weapon ID.</param>
    /// <returns>True if the weapon is in the loadout config, false otherwise.</returns>
    public bool HasWeapon(LoadoutConfig loadoutConfig, int weaponId)
    {
        var builderResult = SqlBuilder
            .From(TableNames.LoadoutWeapon)
            .Where("LoadoutConfigId = @LoadoutConfigId", new { LoadoutConfigId = loadoutConfig.Id })
            .Where("WeaponId = @WeaponId", new { WeaponId = weaponId })
            .BuildCount();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters) > 0;
    }

    /// <summary>
    /// Finds a LoadoutConfig by its identifier and retrieves related data dynamically based on the specified options.
    /// </summary>
    /// <param name="id">The unique identifier of the LoadoutConfig to retrieve.</param>
    /// <param name="options">The options specifying which related data to include.</param>
    /// <returns>The LoadoutConfig associated with the given identifier and related data, or null if not found.</returns>
    public LoadoutConfig FindByIdWithDynamicRelated(int id, LoadoutConfigLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, id).FirstOrDefault();
    }

    /// <summary>
    /// Retrieves a list of all LoadoutConfig entities with related dynamic data based on the specified load options.
    /// </summary>
    /// <param name="options">Options specifying which related data to include in the result.</param>
    /// <returns>A list of LoadoutConfig entities including the related dynamic data as specified in the options.</returns>
    public List<LoadoutConfig> GetAllWithDynamicRelated(LoadoutConfigLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, 0);
    }

    private List<LoadoutConfig> GetAllWithDynamicRelated(LoadoutConfigLoadOptions options, int id)
    {
        var result = SqlBuilder.From($"{this.TableName} lc")
            .Select("lc.*", typeof(LoadoutConfig))
            .Select("lw.*", typeof(LoadoutWeapon))
            .InnerJoin(TableNames.LoadoutWeapon, "lw", "lw.LoadoutConfigId = lc.Id", splitColumn: "LoadoutConfigId")
            .If(options.IncludeWeapons, wb => wb
                .Select("w.*", typeof(Weapon))
                .InnerJoin(TableNames.Weapons, "w", "w.Id = lw.WeaponId", splitColumn: "Id")
                .Select("wwc.*", typeof(WeaponWeaponComponent))
                .LeftJoin(TableNames.WeaponWeaponComponent, "wwc", "wwc.WeaponId = w.Id", splitColumn: "WeaponId")
                .Select("wc.*", typeof(WeaponComponent))
                .LeftJoin(TableNames.WeaponComponents, "wc", "wc.Id = wwc.WeaponComponentId", splitColumn: "Id"))
            .If(id > 0, b => b.Where("lc.Id = @Id", new { Id = id }))
            .Build();

        var lookup = new Dictionary<int, LoadoutConfig>();
        var weaponLookup = new Dictionary<string, LoadoutWeapon>();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Query(
            result.Sql,
            result.Types,
            objArray =>
            {
                var loadoutConfig = objArray[0] as LoadoutConfig;
                var index = 1;
                var loadoutWeapon = objArray[index++] as LoadoutWeapon;

                if (loadoutConfig is null)
                {
                    return null;
                }

                if (!lookup.TryGetValue(loadoutConfig.Id, out var currentConfig))
                {
                    currentConfig = loadoutConfig;
                    currentConfig.LoadoutWeapons = new List<LoadoutWeapon>();
                    lookup.Add(currentConfig.Id, currentConfig);
                }

                if (loadoutWeapon is null)
                {
                    return loadoutConfig;
                }

                var weaponKey = $"{currentConfig.Id}_{loadoutWeapon.WeaponId}";
                if (!weaponLookup.TryGetValue(weaponKey, out var currentLw))
                {
                    currentLw = loadoutWeapon;
                    currentConfig.LoadoutWeapons.Add(currentLw);
                    weaponLookup.Add(weaponKey, currentLw);
                }

                if (!options.IncludeWeapons)
                {
                    return loadoutConfig;
                }

                var weapon = objArray[index++] as Weapon;
                var weaponWeaponComponent = objArray[index++] as WeaponWeaponComponent;
                var weaponComponent = objArray[index] as WeaponComponent;
                if (weapon is null)
                {
                    return loadoutConfig;
                }

                currentLw.Weapon = weapon;
                currentLw.Weapon.WeaponWeaponComponents = new List<WeaponWeaponComponent>();
                if (weaponWeaponComponent is not null && weaponComponent is not null)
                {
                    weaponWeaponComponent.WeaponComponent = weaponComponent;
                    if (currentLw.Weapon.WeaponWeaponComponents.All(c => c.WeaponComponentId != weaponComponent.Id))
                    {
                        currentLw.Weapon.WeaponWeaponComponents.Add(weaponWeaponComponent);
                    }
                }

                return loadoutConfig;
            },
            result.Parameters,
            splitOn: result.SplitOn);

        return lookup.Values.ToList();
    }

#pragma warning disable
    public class LoadoutConfigLoadOptions
    {
        public bool IncludeWeapons { get; set; }
    }
#pragma warning restore
}