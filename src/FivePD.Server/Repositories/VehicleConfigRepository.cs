using Dapper;
using FivePD.Common;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using Newtonsoft.Json;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for VehicleConfig CRUD operations.
/// </summary>
public class VehicleConfigRepository : BaseRepository<VehicleConfig<object>, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleConfigRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public VehicleConfigRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.VehicleConfigs)
    {
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
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var sql = $"SELECT * FROM {this.TableName} WHERE {this.IdColumn} = @Id";
        var a = connection.QueryFirstOrDefault<VehicleConfig<string>>(sql, new { Id = id });
        if (a is null)
        {
            return null;
        }

        return new VehicleConfig<TOptions>
        {
            Id = a.Id,
            Type = a.Type,
            Name = a.Name,
            Model = a.Model,
            Livery = a.Livery,
            Extras = a.Extras,
            Options = a.Options is null
                ? default
                : JsonConvert.DeserializeObject<TOptions>(a.Options),
        };
    }

    /// <summary>
    /// Adds a new vehicle configuration to the database.
    /// </summary>
    /// <typeparam name="TOptions">The type of the Options field.</typeparam>
    /// <param name="vehicleConfig">The vehicle configuration entity to be added.</param>
    /// <returns>The ID of the newly created vehicle configuration.</returns>
    public int Add<TOptions>(VehicleConfig<TOptions> vehicleConfig)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var id = connection.ExecuteScalar<int>(
            $@"
            INSERT INTO {this.TableName} (Name, Type, Model, Livery, Extras, Options)
            VALUES (@Name, @Type, @Model, @Livery, @Extras, @Options);
            SELECT LAST_INSERT_ID();",
            new
            {
                Name = vehicleConfig.Name,
                Type = vehicleConfig.Type,
                Model = vehicleConfig.Model,
                Livery = vehicleConfig.Livery,
                Extras = vehicleConfig.Extras,
                Options = JsonConvert.SerializeObject(vehicleConfig.Options),
            });
        return id;
    }
}