using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for AssistanceServiceStation CRUD operations.
/// </summary>
public class AssistanceServiceStationRepository : BaseRepository<AssistanceServiceStation, AssistanceServiceStationUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssistanceServiceStationRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public AssistanceServiceStationRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.AssistanceServiceStations)
    {
    }

    /// <inheritdoc />
    public override AssistanceServiceStation Add(AssistanceServiceStation entity)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                entity.Name,
                entity.Position,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        entity.Id = id;
        return entity;
    }

#pragma warning disable
    /// <inheritdoc />
    public override void Update(AssistanceServiceStation entity, Action<AssistanceServiceStationUpdate> updateAction)
    {
        var update = new AssistanceServiceStationUpdate();
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
    /// Finds an assistance service station by its identifier and loads related data dynamically based on specified options.
    /// </summary>
    /// <param name="id">The unique identifier of the assistance service station to be retrieved.</param>
    /// <param name="options">The options determining which related data should be included in the retrieval.</param>
    /// <returns>The assistance service station matching the provided identifier, including dynamically loaded related data.</returns>
    public AssistanceServiceStation FindByIdWithDynamicRelated(int id, AssistanceServiceStationLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, id).FirstOrDefault();
    }

    /// <summary>
    /// Retrieves all assistance service stations with dynamically related data based on the provided load options.
    /// </summary>
    /// <param name="options">The options specifying which related data to include.</param>
    /// <returns>A list of assistance service stations with related data based on the specified load options.</returns>
    public List<AssistanceServiceStation> GetAllWithDynamicRelated(AssistanceServiceStationLoadOptions options)
    {
        return this.GetAllWithDynamicRelated(options, 0);
    }

    private List<AssistanceServiceStation> GetAllWithDynamicRelated(AssistanceServiceStationLoadOptions options, int id)
    {
        var result = SqlBuilder.From($"{this.TableName} s")
            .Select("s.*", typeof(AssistanceServiceStation))
            .Select("tc.*", typeof(AssistanceServiceStationTypeConfig))
            .LeftJoin(TableNames.AssistanceServiceStationTypeConfigs, "tc", "s.Id = tc.AssistanceServiceStationId")
            .If(options.IncludeVehicleTypeConfigs, b => b
                .Select("t.*", typeof(AssistanceServiceTypeConfig))
                .LeftJoin(TableNames.AssistanceServiceTypeConfigs, "t", "tc.TypeConfigId = t.Id"))
            .If(id > 0, b => b.WhereEquals("s.Id", id))
            .Build();

        var lookup = new Dictionary<int, AssistanceServiceStation>();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Query(
            result.Sql,
            result.Types,
            objArray =>
            {
                var station = objArray[0] as AssistanceServiceStation;
                var typeConfigLink = objArray[1] as AssistanceServiceStationTypeConfig;
                if (station is null)
                {
                    return null;
                }

                if (!lookup.TryGetValue(station.Id, out var currentStation))
                {
                    currentStation = station;
                    currentStation.TypeConfigs = new List<AssistanceServiceStationTypeConfig>();
                    lookup.Add(currentStation.Id, currentStation);
                }

                if (typeConfigLink is not null)
                {
                    if (currentStation.TypeConfigs.All(x => x.Id != typeConfigLink.Id))
                    {
                        currentStation.TypeConfigs.Add(typeConfigLink);
                    }

                    if (options.IncludeVehicleTypeConfigs && objArray.Length > 2)
                    {
                        var typeConfigDetail = objArray[2] as AssistanceServiceTypeConfig;
                        if (typeConfigDetail != null)
                        {
                            typeConfigLink.TypeConfig = typeConfigDetail;
                        }
                    }
                }

                return currentStation;
            },
            result.Parameters,
            splitOn: result.SplitOn);

        return lookup.Values.ToList();
    }

#pragma warning disable
    public class AssistanceServiceStationLoadOptions
    {
        public bool IncludeVehicleTypeConfigs { get; set; }
    }
#pragma warning restore
}