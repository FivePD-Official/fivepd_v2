using System;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for Location CRUD operations.
/// </summary>
public class LocationRepository : BaseRepository<Location, LocationUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocationRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public LocationRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.Locations)
    {
    }

    /// <inheritdoc />
    public override Location Add(Location location)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new { location.Name, location.Type, location.Position })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        location.Id = id;
        return location;
    }

    /// <inheritdoc />
    public override void Update(Location location, Action<LocationUpdate> updateAction)
    {
        var update = new LocationUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Name), update.Name);
        builder.Add(nameof(update.Type), update.Type);
        builder.Add(nameof(update.X), update.X);
        builder.Add(nameof(update.Y), update.Y);
        builder.Add(nameof(update.Z), update.Z);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, location.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }
}