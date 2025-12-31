using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;
using Newtonsoft.Json;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for VehicleConfig CRUD operations.
/// </summary>
public class VehicleConfigRepository : BaseRepository<VehicleConfig<string>, VehicleConfigUpdate<string>, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleConfigRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public VehicleConfigRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.VehicleConfigs)
    {
    }

    /// <inheritdoc/>
    public override VehicleConfig<string> Add(VehicleConfig<string> entity)
    {
        return this.AddCore(entity);
    }

    /// <summary>
    /// Adds a new vehicle configuration to the repository.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options field in the vehicle configuration.</typeparam>
    /// <param name="vehicleConfig">The vehicle configuration to add.</param>
    /// <returns>The added vehicle configuration, including its assigned ID after persistence.</returns>
    public VehicleConfig<TOptions> Add<TOptions>(VehicleConfig<TOptions> vehicleConfig)
    {
        var rawEntity = new VehicleConfig<string>
        {
            Name = vehicleConfig.Name,
            Type = vehicleConfig.Type,
            Model = vehicleConfig.Model,
            Livery = vehicleConfig.Livery,
            Extras = vehicleConfig.Extras,
            Options = JsonConvert.SerializeObject(vehicleConfig.Options),
        };
        var savedRawEntity = this.AddCore(rawEntity);
        vehicleConfig.Id = savedRawEntity.Id;
        return vehicleConfig;
    }

    /// <inheritdoc />
    public override void Update(VehicleConfig<string> entity, Action<VehicleConfigUpdate<string>> updateAction)
    {
        var update = new VehicleConfigUpdate<string>();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Type), update.Type);
        builder.Add(nameof(update.Name), update.Name);
        builder.Add(nameof(update.Model), update.Model);
        builder.Add(nameof(update.Livery), update.Livery);
        builder.Add(nameof(update.Extras), update.Extras);
        builder.Add(nameof(update.Options), update.Options);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, entity.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }

    /// <summary>
    /// Updates a vehicle configuration with generic options.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options field in the vehicle configuration.</typeparam>
    /// <param name="vehicleConfig">The vehicle configuration to update.</param>
    /// <param name="updateAction">The action to perform on the update model.</param>
    public void Update<TOptions>(VehicleConfig<TOptions> vehicleConfig, Action<VehicleConfigUpdate<TOptions>> updateAction)
    {
        var update = new VehicleConfigUpdate<TOptions>();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Type), update.Type);
        builder.Add(nameof(update.Name), update.Name);
        builder.Add(nameof(update.Model), update.Model);
        builder.Add(nameof(update.Livery), update.Livery);
        builder.Add(nameof(update.Extras), update.Extras);

        if (update.Options is not null)
        {
            builder.Add(nameof(update.Options), JsonConvert.SerializeObject(update.Options));
        }

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, vehicleConfig.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }

    /// <summary>
    /// Retrieves a vehicle configuration by its unique identifier.
    /// </summary>
    /// <typeparam name="TOptions">The type representing the options associated with the vehicle configuration.</typeparam>
    /// <param name="id">The unique identifier of the vehicle configuration to retrieve.</param>
    /// <returns>
    /// An instance of <see cref="VehicleConfig{TOptions}"/> containing the vehicle configuration data
    /// if it exists; otherwise, null.
    /// </returns>
    public VehicleConfig<TOptions> FindById<TOptions>(int id)
    {
        var entity = this.FindById(id);
        return new VehicleConfig<TOptions>
        {
            Id = entity.Id,
            Type = entity.Type,
            Name = entity.Name,
            Model = entity.Model,
            Livery = entity.Livery,
            Extras = entity.Extras,
            Options = entity.Options is null
                ? default
                : JsonConvert.DeserializeObject<TOptions>(entity.Options),
        };
    }

    /// <summary>
    /// Retrieves all vehicle configurations that match the specified condition.
    /// </summary>
    /// <typeparam name="TOptions">The type of options associated with the vehicle configuration.</typeparam>
    /// <param name="queryBuilder">The query builder action to define the condition and parameters.</param>
    /// <returns>A collection of <see cref="VehicleConfig{TOptions}"/> objects that match the specified condition.</returns>
    public IEnumerable<VehicleConfig<TOptions>> GetAll<TOptions>(Action<IQueryBuilder> queryBuilder = null)
    {
        var vehicleConfigs = this.GetAll(queryBuilder).ToList();
        return vehicleConfigs.Select(entity => new VehicleConfig<TOptions>
        {
            Id = entity.Id,
            Type = entity.Type,
            Name = entity.Name,
            Model = entity.Model,
            Livery = entity.Livery,
            Extras = entity.Extras,
            Options = entity.Options is null
                ? default
                : JsonConvert.DeserializeObject<TOptions>(entity.Options),
        });
    }

    private VehicleConfig<string> AddCore(VehicleConfig<string> entity)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                entity.Name,
                entity.Type,
                entity.Model,
                entity.Livery,
                entity.Extras,
                entity.Options,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        entity.Id = id;
        return entity;
    }
}