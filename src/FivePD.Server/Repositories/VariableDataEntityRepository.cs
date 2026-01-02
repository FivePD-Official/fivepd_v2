using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.UpdateModels;
using FivePD.Server.Models.VariableData;

namespace FivePD.Server.Repositories;

#pragma warning disable

/// <summary>
/// Repository for VariableData operations.
/// </summary>
public class VariableDataEntityRepository : BaseRepository<VariableDataEntity, VariableDataEntityUpdate, string>
{
    private readonly VariableDataAttributeRepository _variableDataAttributeRepository;
    private readonly VariableDataValueRepository _variableDataValueRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableDataEntityRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    /// <param name="variableDataAttributeRepository">The variable data attribute repository.</param>
    /// <param name="variableDataValueRepository">The variable data value repository.</param>
    public VariableDataEntityRepository(
        IDatabaseConnectionFactory dbConnectionFactory,
        VariableDataAttributeRepository variableDataAttributeRepository,
        VariableDataValueRepository variableDataValueRepository)
        : base(dbConnectionFactory, TableNames.VariableDataEntities)
    {
        this._variableDataAttributeRepository = variableDataAttributeRepository;
        this._variableDataValueRepository = variableDataValueRepository;
    }

    /// <inheritdoc />
    public override VariableDataEntity Add(VariableDataEntity entity)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                entity.Id,
                entity.SetId,
            })
            .Build();
        connection.Execute(builderResult.Sql, builderResult.Parameters);
        return entity;
    }

    /// <inheritdoc />
    public override void Update(VariableDataEntity entity, Action<VariableDataEntityUpdate> updateAction)
    {
        var update = new VariableDataEntityUpdate();
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
    /// Gets all VariableDataEntities for a set.
    /// </summary>
    /// <param name="setId">The ID of the set.</param>
    /// <returns>A list of VariableDataEntities.</returns>
    public List<VariableDataEntity> GetEntitiesBySetId(string setId)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .From(TableNames.VariableDataEntities)
            .WhereEquals("SetId", setId)
            .Build();
        var entitiesWithoutRequiredProperties = connection.Query<VariableDataEntity>(builderResult.Sql, builderResult.Parameters).ToList();
        var entitiesWithRequiredProperties = new List<VariableDataEntity>();
        foreach (var entity in entitiesWithoutRequiredProperties)
        {
            entitiesWithRequiredProperties.Add(this.GetEntityWithValues(entity.Id));
        }

        return entitiesWithRequiredProperties;
    }

    /// <summary>
    /// Gets a VariableDataEntity by its ID with its values.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>The VariableDataEntity with values.</returns>
    public VariableDataEntity GetEntityWithValues(string id)
    {
        var entity = this.FindById(id);
        if (entity is null)
        {
            return null;
        }

        var builderResult = SqlBuilder
            .From(TableNames.VariableDataSets)
            .WhereEquals("Id", id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        entity.Set = connection.QueryFirstOrDefault<VariableDataSet>(builderResult.Sql, builderResult.Parameters);
        entity.Values = this._variableDataValueRepository.GetValuesByEntityId(id);
        foreach (var value in entity.Values)
        {
            value.Attribute = this._variableDataAttributeRepository.FindById(value.AttributeId);
        }

        return entity;
    }
}