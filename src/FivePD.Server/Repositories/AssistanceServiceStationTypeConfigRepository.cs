using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;
using FivePD.Server.QueryHelpers;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for AssistanceServiceStationTypeConfig CRUD operations.
/// </summary>
public class AssistanceServiceStationTypeConfigRepository : BaseRepository<AssistanceServiceStationTypeConfig, AssistanceServiceStationTypeConfigUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssistanceServiceStationTypeConfigRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public AssistanceServiceStationTypeConfigRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.AssistanceServiceStationTypeConfigs)
    {
    }

    /// <inheritdoc />
    public override AssistanceServiceStationTypeConfig Add(AssistanceServiceStationTypeConfig entity)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                entity.AssistanceServiceStationId,
                entity.TypeConfigId,
                entity.Capacity,
                entity.ActiveCapacity,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        entity.Id = id;
        return entity;
    }

    /// <inheritdoc />
    public override void Update(AssistanceServiceStationTypeConfig entity, Action<AssistanceServiceStationTypeConfigUpdate> updateAction)
    {
        var update = new AssistanceServiceStationTypeConfigUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.AssistanceServiceStationId), update.AssistanceServiceStationId);
        builder.Add(nameof(update.TypeConfigId), update.TypeConfigId);
        builder.Add(nameof(update.Capacity), update.Capacity);
        builder.Add(nameof(update.ActiveCapacity), update.ActiveCapacity);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, entity.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }

    /// <summary>
    /// Resets the active capacity of all assistance service station type configurations.
    /// </summary>
    public void ResetActiveCapacity()
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var sql = $"UPDATE {this.TableName} SET ActiveCapacity = 0";
        connection.Execute(sql);
    }

    /// <summary>
    /// Finds an AssistanceServiceStationTypeConfig by its identifier with dynamically related data based on provided load options.
    /// </summary>
    /// <param name="id">The unique identifier of the AssistanceServiceStationTypeConfig.</param>
    /// <param name="options">The load options specifying which related data to include.</param>
    /// <returns>A single AssistanceServiceStationTypeConfig with dynamically loaded related data if found; otherwise, null.</returns>
    public AssistanceServiceStationTypeConfig FindByIdWithDynamicRelated(int id, AssistanceServiceStationTypeConfigLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, id).FirstOrDefault();
    }

    /// <summary>
    /// Retrieves all instances of <see cref="AssistanceServiceStationTypeConfig"/> with dynamically related data based on the specified load options.
    /// </summary>
    /// <param name="options">The load options that define which related entities to include in the query.</param>
    /// <returns>A list of <see cref="AssistanceServiceStationTypeConfig"/> instances with the related data included as specified by the load options.</returns>
    public List<AssistanceServiceStationTypeConfig> GetAllWithDynamicRelated(AssistanceServiceStationTypeConfigLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, 0);
    }

    private List<AssistanceServiceStationTypeConfig> GetAllWithDynamicRelated(AssistanceServiceStationTypeConfigLoadOptions options, int id)
    {
        var result = SqlBuilder.From($"{this.TableName} tc")
            .Select("tc.*", typeof(AssistanceServiceStationTypeConfig))
            .If(options.IncludeTypeConfig, b => b
                .Select("t.*", typeof(AssistanceServiceTypeConfig))
                .LeftJoin(TableNames.AssistanceServiceTypeConfigs, "t", "tc.TypeConfigId = t.Id"))
            .If(options.IncludeStation, b => b
                .Select("s.*", typeof(AssistanceServiceStation))
                .LeftJoin(TableNames.AssistanceServiceStations, "s", "tc.StationId = t.Id"))
            .If(id > 0, b => b.Where("tc.Id = @Id", new { Id = id }))
            .Build();

        var lookup = new Dictionary<int, AssistanceServiceStationTypeConfig>();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Query(
            result.Sql,
            result.Types,
            objArray =>
            {
                var typeConfigLink = objArray[0] as AssistanceServiceStationTypeConfig;
                if (typeConfigLink is null)
                {
                    return null;
                }

                if (!lookup.TryGetValue(typeConfigLink.Id, out var currentLink))
                {
                    currentLink = typeConfigLink;
                    lookup.Add(currentLink.Id, currentLink);
                }

                int index = 1;
                if (options.IncludeTypeConfig)
                {
                    var typeConfig = objArray[index++] as AssistanceServiceTypeConfig;
                    if (typeConfig != null)
                    {
                        currentLink.TypeConfig = typeConfig;
                    }
                }

                if (options.IncludeStation)
                {
                    var station = objArray[index] as AssistanceServiceStation;
                    if (station != null)
                    {
                        currentLink.AssistanceServiceStation = station;
                    }
                }

                return currentLink;
            },
            result.Parameters,
            splitOn: result.SplitOn);

        return lookup.Values.ToList();
    }

#pragma warning disable
    public class AssistanceServiceStationTypeConfigLoadOptions
    {
        public bool IncludeTypeConfig { get; set; }

        public bool IncludeStation { get; set; }
    }
#pragma warning restore
}
