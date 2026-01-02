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
/// Repository for Department CRUD operations.
/// </summary>
public class DepartmentRepository : BaseRepository<Department, DepartmentUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DepartmentRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public DepartmentRepository(IDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.Departments)
    {
    }

    /// <inheritdoc />
    public override Department Add(Department department)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                department.Name,
                department.Callsign,
                department.MaxUsers,
                department.UseGlobalRanks,
                department.CallsignFormat,
                department.DefaultRankId,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        department.Id = id;
        return department;
    }

    /// <inheritdoc />
    public override void Update(Department department, Action<DepartmentUpdate> updateAction)
    {
        var update = new DepartmentUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builderResult = SqlBuilder
            .Update(this.TableName)
            .SetTracked(update, this.IdColumn, department.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Attaches a rank to a department.
    /// </summary>
    /// <param name="department">The department.</param>
    /// <param name="rank">The rank.</param>
    public void AttachRank(Department department, Rank rank)
    {
        var builderResult = SqlBuilder
            .Into(TableNames.DepartmentRanks)
            .Insert(new
            {
                DepartmentId = department.Id,
                RankId = rank.Id,
            })
            .OnDuplicateKeyUpdate("DepartmentId", "RankId")
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Removes a rank from a department.
    /// </summary>
    /// <param name="department">The department.</param>
    /// <param name="rank">The rank.</param>
    public void RemoveRank(Department department, Rank rank)
    {
        var builderResult = SqlBuilder
            .From(TableNames.DepartmentRanks)
            .Delete()
            .WhereEquals("DepartmentId", department.Id)
            .WhereEquals("RankId", rank.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets all ranks for a department.
    /// </summary>
    /// <param name="department">The department ID.</param>
    /// <returns>A list of ranks.</returns>
    public IEnumerable<Rank> GetRanks(Department department)
    {
        return this.GetRanksById(department.Id);
    }

    /// <summary>
    /// Gets all ranks for a department.
    /// </summary>
    /// <param name="departmentId">The department ID.</param>
    /// <returns>A list of ranks.</returns>
    public IEnumerable<Rank> GetRanksById(int departmentId)
    {
        var builderResult = SqlBuilder.From($"{TableNames.Ranks} r")
            .Select("r.*", typeof(Rank))
            .InnerJoin(TableNames.DepartmentRanks, "dr", "r.Id = dr.RankId", splitColumn: "RankId")
            .WhereEquals("dr.DepartmentId", departmentId)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<Rank>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Retrieves a collection of users associated with the specified department.
    /// </summary>
    /// <param name="department">The department for which to retrieve the associated users.</param>
    /// <returns>A collection of users belonging to the specified department.</returns>
    public IEnumerable<User> GetUsers(Department department)
    {
        return this.GetUsersById(department.Id);
    }

    /// <summary>
    /// Retrieves the users associated with a specific department by its unique identifier.
    /// </summary>
    /// <param name="departmentId">The unique identifier of the department.</param>
    /// <returns>A collection of users assigned to the specified department.</returns>
    public IEnumerable<User> GetUsersById(int departmentId)
    {
        var builderResult = SqlBuilder
            .From(TableNames.Users)
            .WhereEquals("DepartmentId", departmentId)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<User>(builderResult.Sql, builderResult.Parameters);
    }
}