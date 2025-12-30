using System;
using System.Collections.Generic;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.LoadoutModels;
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
    /// Gets all loadout configs that contain a specific weapon.
    /// </summary>
    /// <param name="weapon">The weapon.</param>
    /// <returns>List of loadout configs.</returns>
    public IEnumerable<LoadoutConfig> GetLoadoutConfigsByWeapon(Weapon weapon)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            SELECT lc.* 
            FROM {TableNames.LoadoutConfigs} lc
            INNER JOIN {TableNames.LoadoutWeapon} lw ON lc.Id = lw.LoadoutConfigId
            WHERE lw.WeaponId = @WeaponId";
        return connection.Query<LoadoutConfig>(sql, new { WeaponId = weapon.Id });
    }

    /// <summary>
    /// Checks if a weapon is in a loadout config.
    /// </summary>
    /// <param name="loadoutConfig">The loadout config ID.</param>
    /// <param name="weapon">The weapon ID.</param>
    /// <returns>True if the weapon is in the loadout config, false otherwise.</returns>
    public bool HasWeapon(LoadoutConfig loadoutConfig, Weapon weapon)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            SELECT COUNT(*) 
            FROM {TableNames.LoadoutWeapon}
            WHERE LoadoutConfigId = @LoadoutConfigId AND WeaponId = @WeaponId";
        return connection.ExecuteScalar<int>(sql, new { LoadoutConfigId = loadoutConfig.Id, WeaponId = weapon.Id }) > 0;
    }
}