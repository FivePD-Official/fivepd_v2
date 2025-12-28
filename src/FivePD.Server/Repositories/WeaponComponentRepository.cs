using System.Collections.Generic;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Models.LoadoutModels.JoinTables;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for WeaponComponent CRUD operations.
/// </summary>
public class WeaponComponentRepository : BaseRepository<Rank, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponComponentRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public WeaponComponentRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.WeaponComponents)
    {
    }

    /// <summary>
    /// Adds a new weapon component to the database.
    /// </summary>
    /// <param name="weaponComponent">The weapon component entity to be added.</param>
    /// <returns>The ID of the newly created weapon component.</returns>
    public WeaponComponent Add(WeaponComponent weaponComponent)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var id = connection.ExecuteScalar<int>(
            $@"
            INSERT INTO {this.TableName} (Name, HashKey)
            VALUES (@Name, @HashKey);
            SELECT LAST_INSERT_ID();",
            new
            {
                Name = weaponComponent.Name,
                HashKey = weaponComponent.HashKey,
            });
        weaponComponent.Id = id;
        return weaponComponent;
    }

    /// <summary>
    /// Attaches a weapon to this component.
    /// </summary>
    /// <param name="component">The weapon component.</param>
    /// <param name="weapon">The weapon.</param>
    public void AttachWeapon(WeaponComponent component, Weapon weapon)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            INSERT INTO {TableNames.WeaponWeaponComponent} (WeaponId, WeaponComponentId)
            VALUES (@WeaponId, @ComponentId)
            ON DUPLICATE KEY UPDATE WeaponId = @WeaponId";
        connection.Execute(sql, new { WeaponId = weapon.Id, ComponentId = component.Id });
    }

    /// <summary>
    /// Removes a weapon from this component.
    /// </summary>
    /// <param name="component">The weapon component.</param>
    /// <param name="weapon">The weapon.</param>
    public void RemoveWeapon(WeaponComponent component, Weapon weapon)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            DELETE FROM {TableNames.WeaponWeaponComponent}
            WHERE WeaponId = @WeaponId AND WeaponComponentId = @ComponentId";
        connection.Execute(sql, new { WeaponId = weapon.Id, ComponentId = component.Id });
    }

    /// <summary>
    /// Gets all weapons that have this component attached.
    /// </summary>
    /// <param name="component">The weapon component.</param>
    /// <returns>List of weapons.</returns>
    public IEnumerable<Weapon> GetWeapons(WeaponComponent component)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            SELECT w.* 
            FROM {TableNames.Weapons} w
            INNER JOIN {TableNames.WeaponWeaponComponent} wwc ON w.Id = wwc.WeaponId
            WHERE wwc.WeaponComponentId = @ComponentId";
        return connection.Query<Weapon>(sql, new { ComponentId = component.Id });
    }

    /// <summary>
    /// Checks if a component is attached to a specific weapon.
    /// </summary>
    /// <param name="component">The weapon component.</param>
    /// <param name="weapon">The weapon.</param>
    /// <returns>True if the component is attached to the weapon, false otherwise.</returns>
    public bool IsAttachedToWeapon(WeaponComponent component, Weapon weapon)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            SELECT COUNT(*) 
            FROM {TableNames.WeaponWeaponComponent}
            WHERE WeaponId = @WeaponId AND WeaponComponentId = @ComponentId";
        return connection.ExecuteScalar<int>(sql, new { WeaponId = weapon.Id, ComponentId = component.Id }) > 0;
    }
}