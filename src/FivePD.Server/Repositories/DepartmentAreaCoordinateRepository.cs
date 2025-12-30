using System;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for DepartmentAreaCoordinate CRUD operations.
/// </summary>
public class DepartmentAreaCoordinateRepository : BaseRepository<DepartmentAreaCoordinate, DepartmentAreaCoordinateUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DepartmentAreaCoordinateRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public DepartmentAreaCoordinateRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.DepartmentAreaCoordinates)
    {
    }

    /// <inheritdoc />
    public override DepartmentAreaCoordinate Add(DepartmentAreaCoordinate coordinate)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                coordinate.DepartmentId,
                coordinate.X,
                coordinate.Y,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        coordinate.Id = id;
        return coordinate;
    }

    /// <inheritdoc />
    public override void Update(DepartmentAreaCoordinate coordinate, Action<DepartmentAreaCoordinateUpdate> updateAction)
    {
        var update = new DepartmentAreaCoordinateUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.DepartmentId), update.DepartmentId);
        builder.Add(nameof(update.X), update.X);
        builder.Add(nameof(update.Y), update.Y);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, coordinate.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }
}