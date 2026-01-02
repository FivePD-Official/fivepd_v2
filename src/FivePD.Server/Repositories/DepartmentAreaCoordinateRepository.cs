using System;
using System.Collections.Generic;
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
    public DepartmentAreaCoordinateRepository(IDatabaseConnectionFactory dbConnectionFactory)
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

        var builderResult = SqlBuilder
            .Update(this.TableName)
            .SetTracked(update, this.IdColumn, coordinate.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Retrieves all area coordinates for a specific department.
    /// </summary>
    /// <param name="departmentId">The unique identifier of the department.</param>
    /// <returns>A collection of area coordinates associated with the specified department.</returns>
    public IEnumerable<DepartmentAreaCoordinate> GetAllByDepartmentId(int departmentId)
    {
        var builderResult = SqlBuilder
            .From(this.TableName)
            .WhereEquals("DepartmentId", departmentId)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<DepartmentAreaCoordinate>(builderResult.Sql, builderResult.Parameters);
    }
}