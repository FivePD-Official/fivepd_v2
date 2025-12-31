using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Models.LoadoutModels.JoinTables;
using FivePD.Server.Models.UpdateModels;
using Newtonsoft.Json;

namespace FivePD.Server.Repositories;

#pragma warning disable

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
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            INSERT INTO {TableNames.LoadoutWeapon} (LoadoutConfigId, WeaponId)
            VALUES (@LoadoutConfigId, @WeaponId)
            ON DUPLICATE KEY UPDATE LoadoutConfigId = @LoadoutConfigId";
        connection.Execute(sql, new { LoadoutConfigId = loadoutConfig.Id, WeaponId = weapon.Id });
    }

    /// <summary>
    /// Removes a weapon from a loadout config.
    /// </summary>
    /// <param name="loadoutConfig">The loadout config.</param>
    /// <param name="weapon">The weapon.</param>
    public void RemoveWeapon(LoadoutConfig loadoutConfig, Weapon weapon)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            DELETE FROM {TableNames.LoadoutWeapon}
            WHERE LoadoutConfigId = @LoadoutConfigId AND WeaponId = @WeaponId";
        connection.Execute(sql, new { LoadoutConfigId = loadoutConfig.Id, WeaponId = weapon.Id });
    }

    /// <summary>
    /// Gets all weapons in a loadout config.
    /// </summary>
    /// <param name="loadoutConfig">The loadout config.</param>
    /// <returns>List of weapons.</returns>
    public IEnumerable<Weapon> GetWeapons(LoadoutConfig loadoutConfig)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            SELECT w.* 
            FROM {TableNames.Weapons} w
            INNER JOIN {TableNames.LoadoutWeapon} lw ON w.Id = lw.WeaponId
            WHERE lw.LoadoutConfigId = @LoadoutConfigId";
        return connection.Query<Weapon>(sql, new { LoadoutConfigId = loadoutConfig.Id });
    }

    /// <summary>
    /// Checks if a weapon is in a loadout config.
    /// </summary>
    /// <param name="loadoutConfig">The loadout config ID.</param>
    /// <param name="weaponId">The weapon ID.</param>
    /// <returns>True if the weapon is in the loadout config, false otherwise.</returns>
    public bool HasWeapon(LoadoutConfig loadoutConfig, int weaponId)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            SELECT COUNT(*) 
            FROM {TableNames.LoadoutWeapon}
            WHERE LoadoutConfigId = @LoadoutConfigId AND WeaponId = @WeaponId";
        return connection.ExecuteScalar<int>(sql, new { LoadoutConfigId = loadoutConfig.Id, WeaponId = weaponId }) > 0;
    }

    public LoadoutConfig FindByIdWithDynamicRelated(int id, LoadoutConfigLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, id).FirstOrDefault();
    }

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
            splitOn: result.SplitOn
        );

        return lookup.Values.ToList();
    }

    public class LoadoutConfigLoadOptions
    {
        public bool IncludeWeapons { get; set; }
    }
}