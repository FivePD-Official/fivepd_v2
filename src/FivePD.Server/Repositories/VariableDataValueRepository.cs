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

public class VariableDataValueRepository : BaseRepository<VariableDataValue, VariableDataValueUpdate, int>
{
    public VariableDataValueRepository(IDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.VariableDataValues)
    {
    }

    public override VariableDataValue Add(VariableDataValue value)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                value.EntityId,
                value.AttributeId,
                value.Value,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        value.Id = id;
        return value;
    }

    public override void Update(VariableDataValue value, Action<VariableDataValueUpdate> updateAction)
    {
        var update = new VariableDataValueUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Update(this.TableName)
            .SetTracked(update, "Id", value.Id)
            .Build();
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets all VariableDataValues for an entity.
    /// </summary>
    /// <param name="entityId">The ID of the entity.</param>
    /// <returns>A list of VariableDataValues.</returns>
    public List<VariableDataValue> GetValuesByEntityId(string entityId)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .From(this.TableName)
            .WhereEquals("EntityId", entityId)
            .Build();
        return connection.Query<VariableDataValue>(builderResult.Sql, builderResult.Parameters).ToList();
    }

    /// <summary>
    /// Gets all VariableDataValues for an attribute.
    /// </summary>
    /// <param name="attributeId">The ID of the attribute.</param>
    /// <returns>A list of VariableDataValues.</returns>
    public List<VariableDataValue> GetValuesByAttributeId(int attributeId)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .From(this.TableName)
            .WhereEquals("AttributeId", attributeId)
            .Build();
        return connection.Query<VariableDataValue>(builderResult.Sql, builderResult.Parameters).ToList();
    }

    /// <summary>
    /// Deletes all VariableDataValues for an entity.
    /// </summary>
    /// <param name="entityId">The ID of the entity.</param>
    public void DeleteValuesByEntityId(string entityId)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .From(this.TableName)
            .Delete()
            .WhereEquals("EntityId", entityId)
            .Build();
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Deletes all VariableDataValues for an attribute.
    /// </summary>
    /// <param name="attributeId">The ID of the attribute.</param>
    public void DeleteValuesByAttributeId(int attributeId)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .From(this.TableName)
            .Delete()
            .WhereEquals("AttributeId", attributeId)
            .Build();
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }
}