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
/// Repository for WeaponComponent CRUD operations.
/// </summary>
public class WeaponComponentRepository : BaseRepository<WeaponComponent, WeaponComponentUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponComponentRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public WeaponComponentRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.WeaponComponents)
    {
    }

    /// <inheritdoc />
    public override WeaponComponent Add(WeaponComponent weaponComponent)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new { weaponComponent.Name, weaponComponent.HashKey, })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        weaponComponent.Id = id;
        return weaponComponent;
    }

    /// <inheritdoc />
    public override void Update(WeaponComponent weaponComponent, Action<WeaponComponentUpdate> updateAction)
    {
        var update = new WeaponComponentUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Name), update.Name);
        builder.Add(nameof(update.HashKey), update.HashKey);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, weaponComponent.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }

    /// <summary>
    /// Finds a weapon component by its unique hash key.
    /// </summary>
    /// <param name="hashKey">The unique hash key identifying the weapon component.</param>
    /// <returns>
    /// The <see cref="WeaponComponent"/> corresponding to the provided hash key,
    /// or <c>null</c> if no matching component is found.
    /// </returns>
    public WeaponComponent FindByHashKey(string hashKey)
    {
        var builderResult = SqlBuilder
            .From(this.TableName)
            .Where("HashKey = @HashKey", new { HashKey = hashKey })
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.QueryFirstOrDefault<WeaponComponent>(builderResult.Sql, builderResult.Parameters);
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
        var builderResult = SqlBuilder.From($"{TableNames.Weapons} w")
            .Select("w.*", typeof(Weapon))
            .InnerJoin(TableNames.WeaponWeaponComponent, "wwc", "w.Id = wwc.WeaponId", splitColumn: "WeaponId")
            .Where("wwc.WeaponComponentId = @ComponentId", new { ComponentId = component.Id })
            .Build();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<Weapon>(builderResult.Sql, builderResult.Parameters);
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