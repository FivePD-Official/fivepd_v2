using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.UpdateModels;
using FivePD.Server.Models.VariableData;
using Newtonsoft.Json;

namespace FivePD.Server.Repositories;

#pragma warning disable

public class VariableDataAttributeRepository : BaseRepository<VariableDataAttribute, VariableDataAttributeUpdate, int>
{
    public VariableDataAttributeRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.VariableDataAttributes)
    {
    }

    public override VariableDataAttribute Add(VariableDataAttribute attribute)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                attribute.SetId,
                attribute.DataType,
                attribute.AddonId,
                attribute.Name,
                attribute.DisplayOrder,
                attribute.Required,
                attribute.Hidden,
                attribute.Readonly,
                attribute.ShowInGrid,
                attribute.Placeholder,
                attribute.Label,
                attribute.HelpLabel,
                Options = attribute.Options is null ? null : JsonConvert.SerializeObject(attribute.Options),
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        attribute.Id = id;
        return attribute;
    }

    public override void Update(VariableDataAttribute attribute, Action<VariableDataAttributeUpdate> updateAction)
    {
        var update = new VariableDataAttributeUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Update(this.TableName)
            .SetTracked(update, "Id", attribute.Id)
            .Build();
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Retrieves a specific variable data attribute by its set ID and name.
    /// </summary>
    /// <param name="setId">The unique identifier of the variable data set.</param>
    /// <param name="name">The name of the variable data attribute to retrieve.</param>
    /// <returns>The matching <see cref="VariableDataAttribute"/>, or null if no match is found.</returns>
    public VariableDataAttribute GetAttributeBySetIdAndName(string setId, string name)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .From(this.TableName)
            .WhereEquals("SetId", setId)
            .WhereEquals("Name", name)
            .Build();
        return connection.QueryFirstOrDefault<VariableDataAttribute>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets all VariableDataAttributes for a set.
    /// </summary>
    /// <param name="setId">The ID of the set.</param>
    /// <returns>A list of VariableDataAttributes.</returns>
    public List<VariableDataAttribute> GetAttributesBySetId(string setId)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .From(this.TableName)
            .WhereEquals("SetId", setId)
            .Build();
        return connection.Query<VariableDataAttribute>(builderResult.Sql, builderResult.Parameters).ToList();
    }
}