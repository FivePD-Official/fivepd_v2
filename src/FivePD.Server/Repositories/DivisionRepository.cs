using System;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for Division CRUD operations.
/// </summary>
public class DivisionRepository : BaseRepository<Division, DivisionUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DivisionRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public DivisionRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.Divisions)
    {
    }

    /// <inheritdoc />
    public override Division Add(Division division)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var id = connection.ExecuteScalar<int>(
            $@"INSERT INTO {this.TableName} (Name, DepartmentId)
            VALUES (@Name, @DepartmentId);
            SELECT LAST_INSERT_ID();",
            new
            {
                Name = division.Name,
                DepartmentId = division.DepartmentId,
            });
        division.Id = id;
        return division;
    }

    /// <inheritdoc />
    public override void Update(Division division, Action<DivisionUpdate> updateAction)
    {
        var update = new DivisionUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Name), update.Name);
        builder.Add(nameof(update.DepartmentId), update.DepartmentId);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, division.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }
}