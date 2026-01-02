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
/// Repository for Rank CRUD operations.
/// </summary>
public class RankRepository : BaseRepository<Rank, RankUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RankRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public RankRepository(IDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.Ranks)
    {
    }

    /// <inheritdoc />
    public override Rank Add(Rank rank)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                rank.Global,
                rank.RequiredXp,
                rank.Name,
                rank.Permission,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        rank.Id = id;
        return rank;
    }

    /// <inheritdoc />
    public override void Update(Rank rank, Action<RankUpdate> updateAction)
    {
        var update = new RankUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builderResult = SqlBuilder
            .Update(this.TableName)
            .SetTracked(update, this.IdColumn, rank.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets all ranks where the Global property is true.
    /// </summary>
    /// <returns>A list of global ranks.</returns>
    public IEnumerable<Rank> GetAllGlobal()
    {
        return this.GetAll(b => b.Where("Global = @Global", new { Global = true }));
    }

    /// <summary>
    /// Retrieves a collection of ranks associated with a specific department.
    /// </summary>
    /// <param name="rankName">The name of the rank to match.</param>
    /// <param name="departmentId">The identifier of the department for which ranks are being queried.</param>
    /// <param name="rankId">An optional parameter for excluding a specific rank by its identifier.</param>
    /// <returns>A collection of <see cref="Rank"/> objects matching the provided criteria.</returns>
    public IEnumerable<Rank> GetRanksByDepartment(string rankName, int departmentId, int? rankId = null)
    {
        var builderResult = SqlBuilder.From($"{this.TableName} r")
            .Select("r.*", typeof(Rank))
            .InnerJoin(TableNames.DepartmentRanks, "dr", "r.Id = dr.RankId", splitColumn: "RankId")
            .WhereEquals("r.Name", rankName)
            .WhereEquals("r.Global", 0)
            .WhereEquals("dr.DepartmentId", departmentId)
            .If(rankId is not null, b => b.WhereNotEquals("r.Id", rankId))
            .Build();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<Rank>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Attaches a department to a rank.
    /// </summary>
    /// <param name="rank">The rank.</param>
    /// <param name="department">The department.</param>
    public void AttachDepartment(Rank rank, Department department)
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
    /// Removes a department from a rank.
    /// </summary>
    /// <param name="rank">The rank.</param>
    /// <param name="department">The department.</param>
    public void RemoveDepartment(Rank rank, Department department)
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
    /// Gets all departments for a rank.
    /// </summary>
    /// <param name="rank">The rank.</param>
    /// <returns>A list of departments.</returns>
    public IEnumerable<Department> GetDepartments(Rank rank)
    {
        return this.GetDepartmentsById(rank.Id);
    }

    /// <summary>
    /// Gets all departments for a rank.
    /// </summary>
    /// <param name="rankId">The department ID.</param>
    /// <returns>A list of departments.</returns>
    public IEnumerable<Department> GetDepartmentsById(int rankId)
    {
        var builderResult = SqlBuilder.From($"{TableNames.Departments} d")
            .Select("d.*", typeof(Department))
            .InnerJoin(TableNames.DepartmentRanks, "dr", "d.Id = dr.DepartmentId", splitColumn: "DepartmentId")
            .WhereEquals("dr.RankId", rankId)
            .Build();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<Department>(builderResult.Sql, builderResult.Parameters);
    }
}