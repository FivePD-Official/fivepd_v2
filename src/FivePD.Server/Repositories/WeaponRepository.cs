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
/// Repository for Weapon CRUD operations.
/// </summary>
public class WeaponRepository : BaseRepository<Weapon, WeaponUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public WeaponRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.Weapons)
    {
    }

    /// <inheritdoc />
    public override Weapon Add(Weapon weapon)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new { weapon.Name, weapon.Model, })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        weapon.Id = id;
        return weapon;
    }

    /// <inheritdoc />
    public override void Update(Weapon weapon, Action<WeaponUpdate> updateAction)
    {
        var update = new WeaponUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Name), update.Name);
        builder.Add(nameof(update.Model), update.Model);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, weapon.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }

    /// <summary>
    /// Attaches a weapon component to a weapon.
    /// </summary>
    /// <param name="weapon">The weapon.</param>
    /// <param name="component">The weapon component.</param>
    public void AttachComponent(Weapon weapon, WeaponComponent component)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            INSERT INTO {TableNames.WeaponWeaponComponent} (WeaponId, WeaponComponentId)
            VALUES (@WeaponId, @ComponentId)
            ON DUPLICATE KEY UPDATE WeaponId = @WeaponId";
        connection.Execute(sql, new { WeaponId = weapon.Id, ComponentId = component.Id });
    }

    /// <summary>
    /// Removes a weapon component from a weapon.
    /// </summary>
    /// <param name="weapon">The weapon.</param>
    /// <param name="component">The weapon component.</param>
    public void RemoveComponent(Weapon weapon, WeaponComponent component)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            DELETE FROM {TableNames.WeaponWeaponComponent}
            WHERE WeaponId = @WeaponId AND WeaponComponentId = @ComponentId";
        connection.Execute(sql, new { WeaponId = weapon.Id, ComponentId = component.Id });
    }

    /// <summary>
    /// Gets all components attached to a weapon.
    /// </summary>
    /// <param name="weapon">The weapon ID.</param>
    /// <returns>List of weapon components.</returns>
    public IEnumerable<WeaponComponent> GetComponents(Weapon weapon)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            SELECT wc.* 
            FROM {TableNames.WeaponComponents} wc
            INNER JOIN {TableNames.WeaponWeaponComponent} wwc ON wc.Id = wwc.WeaponComponentId
            WHERE wwc.WeaponId = @WeaponId";
        return connection.Query<WeaponComponent>(sql, new { WeaponId = weapon.Id });
    }

    /// <summary>
    /// Checks if a weapon has a specific component attached.
    /// </summary>
    /// <param name="weapon">The weapon.</param>
    /// <param name="component">The weapon component.</param>
    /// <returns>True if the component is attached, false otherwise.</returns>
    public bool HasComponent(Weapon weapon, WeaponComponent component)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            SELECT COUNT(*) 
            FROM {TableNames.WeaponWeaponComponent}
            WHERE WeaponId = @WeaponId AND WeaponComponentId = @ComponentId";
        return connection.ExecuteScalar<int>(sql, new { WeaponId = weapon.Id, ComponentId = component.Id }) > 0;
    }
}