using System;
using System.Collections.Generic;
using Dapper;
using FivePD.Server.Database;
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
    public RankRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.Ranks)
    {
    }

    /// <inheritdoc />
    public override Rank Add(Rank rank)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var id = connection.ExecuteScalar<int>(
            $@"INSERT INTO {this.TableName} (Global, RequiredXp, Name, Permission)
            VALUES (@Global, @RequiredXp, @Name, @Permission);
            SELECT LAST_INSERT_ID();",
            new
            {
                Global = rank.Global,
                RequiredXp = rank.RequiredXp,
                Name = rank.Name,
                Permission = rank.Permission,
            });
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

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Global), update.Global);
        builder.Add(nameof(update.RequiredXp), update.RequiredXp);
        builder.Add(nameof(update.Name), update.Name);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, rank.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }

    /// <summary>
    /// Gets all ranks where the Global property is true.
    /// </summary>
    /// <returns>A list of global ranks.</returns>
    public IEnumerable<Rank> GetAllGlobal()
    {
        return this.GetAllWhere("Global = @Global", new { Global = true });
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
        var join = $"INNER JOIN {TableNames.DepartmentRanks} dr ON {this.TableName}.Id = dr.RankId";
        var condition = $"{this.TableName}.Name = @Name AND {this.TableName}.Global = 0 AND dr.DepartmentId = @DepartmentId";
        if (rankId is not null)
        {
            condition += $" AND {this.TableName}.Id != @RankId";
        }

        return this.GetAllWithJoin(join, condition, new { Name = rankName, DepartmentId = departmentId, RankId = rankId });
    }

    /// <summary>
    /// Attaches a department to a rank.
    /// </summary>
    /// <param name="rank">The rank.</param>
    /// <param name="department">The department.</param>
    public void AttachDepartment(Rank rank, Department department)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            INSERT INTO {TableNames.DepartmentRanks} (DepartmentId, RankId)
            VALUES (@DepartmentId, @RankId)
            ON DUPLICATE KEY UPDATE RankId = @RankId";
        connection.Execute(sql, new { DepartmentId = department.Id, RankId = rank.Id });
    }

    /// <summary>
    /// Removes a department from a rank.
    /// </summary>
    /// <param name="rank">The rank.</param>
    /// <param name="department">The department.</param>
    public void RemoveDepartment(Rank rank, Department department)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            DELETE FROM {TableNames.DepartmentRanks}
            WHERE RankId = @RankId AND DepartmentId = @DepartmentId";
        connection.Execute(sql, new { RankId = rank.Id, DepartmentId = department.Id });
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
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            SELECT d.*
            FROM {TableNames.DepartmentRanks} dr
            INNER JOIN {TableNames.Departments} d ON dr.DepartmentId = d.Id
            WHERE dr.RankId = @RankId";
        return connection.Query<Department>(sql, new { RankId = rankId });
    }
}