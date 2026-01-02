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
/// Repository for Weapon CRUD operations.
/// </summary>
public class WeaponRepository : BaseRepository<Weapon, WeaponUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public WeaponRepository(IDatabaseConnectionFactory dbConnectionFactory)
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

        var builderResult = SqlBuilder
            .Update(this.TableName)
            .SetTracked(update, this.IdColumn, weapon.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Attaches a weapon component to a weapon.
    /// </summary>
    /// <param name="weapon">The weapon.</param>
    /// <param name="component">The weapon component.</param>
    public void AttachComponent(Weapon weapon, WeaponComponent component)
    {
        var builderResult = SqlBuilder
            .Into(TableNames.WeaponWeaponComponent)
            .Insert(new
            {
                WeaponId = weapon.Id,
                WeaponComponentId = component.Id,
            })
            .OnDuplicateKeyUpdate("WeaponId", "WeaponComponentId")
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Removes a weapon component from a weapon.
    /// </summary>
    /// <param name="weapon">The weapon.</param>
    /// <param name="component">The weapon component.</param>
    public void RemoveComponent(Weapon weapon, WeaponComponent component)
    {
        var builderResult = SqlBuilder
            .From(TableNames.WeaponWeaponComponent)
            .WhereEquals("WeaponId", weapon.Id)
            .WhereEquals("WeaponComponentId", component.Id)
            .Delete()
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets all components attached to a weapon.
    /// </summary>
    /// <param name="weapon">The weapon ID.</param>
    /// <returns>List of weapon components.</returns>
    public IEnumerable<WeaponComponent> GetComponents(Weapon weapon)
    {
        var builderResult = SqlBuilder
            .From($"{TableNames.WeaponComponents} wc")
            .Select("wc.*")
            .InnerJoin(TableNames.WeaponWeaponComponent, "wwc", "wc.Id = wwc.WeaponComponentId")
            .WhereEquals("wwc.WeaponId", weapon.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<WeaponComponent>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets all loadout configs that contain a specific weapon.
    /// </summary>
    /// <param name="weapon">The weapon.</param>
    /// <returns>List of loadout configs.</returns>
    public IEnumerable<LoadoutConfig> GetLoadoutConfigsByWeapon(Weapon weapon)
    {
        var builderResult = SqlBuilder
            .From($"{TableNames.LoadoutConfigs} lc")
            .Select("lc.*")
            .InnerJoin(TableNames.LoadoutWeapon, "lw", "lc.Id = lw.LoadoutConfigId")
            .WhereEquals("lw.WeaponId", weapon.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<LoadoutConfig>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Checks if a weapon has a specific component attached.
    /// </summary>
    /// <param name="weapon">The weapon.</param>
    /// <param name="component">The weapon component.</param>
    /// <returns>True if the component is attached, false otherwise.</returns>
    public bool HasComponent(Weapon weapon, WeaponComponent component)
    {
        var builderResult = SqlBuilder
            .From(TableNames.WeaponWeaponComponent)
            .WhereEquals("WeaponId", weapon.Id)
            .WhereEquals("WeaponComponentId", component.Id)
            .BuildCount();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters) > 0;
    }

    /// <summary>
    /// Finds a weapon by its ID and includes related dynamic data based on the specified load options.
    /// </summary>
    /// <param name="id">The ID of the weapon to find.</param>
    /// <param name="options">The load options to include related dynamic data.</param>
    /// <returns>The weapon with the specified ID, including the related dynamic data defined by the load options. Returns null if no weapon is found.</returns>
    public Weapon FindByIdWithDynamicRelated(int id, WeaponLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, id).FirstOrDefault();
    }

    /// <summary>
    /// Retrieves all weapon records with dynamically loaded related data based on the specified options.
    /// </summary>
    /// <param name="options">The options that specify which related data to include in the result set.</param>
    /// <returns>A list of weapons with related data based on the provided options.</returns>
    public List<Weapon> GetAllWithDynamicRelated(WeaponLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, 0);
    }

    private List<Weapon> GetAllWithDynamicRelated(WeaponLoadOptions options, int id)
    {
        var result = SqlBuilder.From($"{this.TableName} w")
            .Select("w.*", typeof(Weapon))
            .If(options.IncludeWeaponComponents, b => b
                .Select("wwc.*", typeof(WeaponWeaponComponent))
                .LeftJoin(TableNames.WeaponWeaponComponent, "wwc", "wwc.WeaponId = w.Id", splitColumn: "WeaponId")
                .Select("wc.*", typeof(WeaponComponent))
                .LeftJoin(TableNames.WeaponComponents, "wc", "wc.Id = wwc.WeaponComponentId", splitColumn: "Id"))
            .If(options.IncludeLoadoutWeapons, b => b
                .Select("lw.*", typeof(LoadoutWeapon))
                .LeftJoin(TableNames.LoadoutWeapon, "lw", "lw.WeaponId = w.Id", splitColumn: "WeaponId"))
            .If(id > 0, b => b.WhereEquals("w.Id", id))
            .Build();

        var lookup = new Dictionary<int, Weapon>();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Query(
            result.Sql,
            result.Types,
            objArray =>
            {
                var weapon = objArray[0] as Weapon;
                var index = 1;

                if (weapon is null)
                {
                    return null;
                }

                if (!lookup.TryGetValue(weapon.Id, out var currentWeapon))
                {
                    currentWeapon = weapon;
                    if (options.IncludeWeaponComponents)
                    {
                        currentWeapon.WeaponWeaponComponents = new List<WeaponWeaponComponent>();
                    }

                    if (options.IncludeLoadoutWeapons)
                    {
                        currentWeapon.LoadoutWeapons = new List<LoadoutWeapon>();
                    }

                    lookup.Add(currentWeapon.Id, currentWeapon);
                }

                if (options.IncludeWeaponComponents)
                {
                    var wwc = objArray[index++] as WeaponWeaponComponent;
                    var wc = objArray[index++] as WeaponComponent;
                    if (wwc is not null && wc is not null)
                    {
                        wwc.WeaponComponent = wc;
                        if (currentWeapon.WeaponWeaponComponents.All(x => x.WeaponComponentId != wc.Id))
                        {
                            currentWeapon.WeaponWeaponComponents.Add(wwc);
                        }
                    }
                }

                if (options.IncludeLoadoutWeapons)
                {
                    var lw = objArray[index] as LoadoutWeapon;
                    if (lw is not null && currentWeapon.LoadoutWeapons.All(x => x.LoadoutConfigId != lw.LoadoutConfigId))
                    {
                        currentWeapon.LoadoutWeapons.Add(lw);
                    }
                }

                return weapon;
            },
            result.Parameters,
            splitOn: result.SplitOn);

        return lookup.Values.ToList();
    }

#pragma warning disable
    public class WeaponLoadOptions
    {
        public bool IncludeWeaponComponents { get; set; }

        public bool IncludeLoadoutWeapons { get; set; }
    }
#pragma warning restore
}