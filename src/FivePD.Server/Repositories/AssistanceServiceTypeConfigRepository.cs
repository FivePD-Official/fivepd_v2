using System;
using CitizenFX.Core;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for AssistanceServiceTypeConfig CRUD operations.
/// </summary>
public class AssistanceServiceTypeConfigRepository : BaseRepository<AssistanceServiceTypeConfig, AssistanceServiceTypeConfigUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssistanceServiceTypeConfigRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public AssistanceServiceTypeConfigRepository(IDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.AssistanceServiceTypeConfigs)
    {
    }

    /// <inheritdoc />
    public override AssistanceServiceTypeConfig Add(AssistanceServiceTypeConfig entity)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                entity.TypeName,
                entity.AssemblyName,
                entity.Active,
                entity.Enabled,
                entity.SpawnType,
                entity.ManualSpawnLocations,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        entity.Id = id;
        return entity;
    }

    /// <inheritdoc />
    public override void Update(AssistanceServiceTypeConfig entity, Action<AssistanceServiceTypeConfigUpdate> updateAction)
    {
        var update = new AssistanceServiceTypeConfigUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builderResult = SqlBuilder
            .Update(this.TableName)
            .SetTracked(update, this.IdColumn, entity.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets an assistance service type configuration by its type name and assembly name.
    /// </summary>
    /// <param name="typeName">The name of the type.</param>
    /// <param name="assemblyName">The name of the assembly.</param>
    /// <returns>The configuration if found, null otherwise.</returns>
    public AssistanceServiceTypeConfig GetByTypeNameAndAssembly(string typeName, string assemblyName)
    {
        var builderResult = SqlBuilder
            .From(this.TableName)
            .WhereEquals("TypeName", typeName)
            .WhereEquals("AssemblyName", assemblyName)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.QueryFirstOrDefault<AssistanceServiceTypeConfig>(builderResult.Sql, builderResult.Parameters);
    }
}