using System;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.UpdateModels;
using FivePD.Server.Models.VariableData;

namespace FivePD.Server.Repositories;

#pragma warning disable

public class VariableDataSetRepository : BaseRepository<VariableDataSet, VariableDataSetUpdate, string>
{
    private readonly VariableDataEntityRepository _variableDataEntityRepository;
    private readonly VariableDataAttributeRepository _variableDataAttributeRepository;
    
    public VariableDataSetRepository(
        IDatabaseConnectionFactory dbConnectionFactory,
        VariableDataEntityRepository variableDataEntityRepository,
        VariableDataAttributeRepository variableDataAttributeRepository)
        : base(dbConnectionFactory, TableNames.VariableDataSets)
    {
        this._variableDataEntityRepository = variableDataEntityRepository;
        this._variableDataAttributeRepository = variableDataAttributeRepository;
    }

    public override VariableDataSet Add(VariableDataSet set)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                set.Id,
            })
            .Build();
        connection.Execute(builderResult.Sql, builderResult.Parameters);
        return set;
    }

    public override void Update(VariableDataSet set, Action<VariableDataSetUpdate> updateAction)
    {
        var update = new VariableDataSetUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Update(this.TableName)
            .SetTracked(update, "Id", set.Id)
            .Build();
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets a VariableDataSet by its ID with its attributes and entities.
    /// </summary>
    /// <param name="id">The ID of the set.</param>
    /// <returns>The VariableDataSet with attributes and entities.</returns>
    public VariableDataSet GetSetWithAttributesAndEntities(string id)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;

        // Get the set
        var set = this.FindById(id);
        if (set is null)
        {
            return null;
        }

        set.Attributes = this._variableDataAttributeRepository.GetAttributesBySetId(id);
        set.Entities = this._variableDataEntityRepository.GetEntitiesBySetId(id);

        return set;
    }
}