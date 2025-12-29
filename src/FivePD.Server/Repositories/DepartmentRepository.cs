using System;
using System.Collections.Generic;
using Dapper;
using FivePD.Server.Database;
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
    public DepartmentRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.Departments)
    {
    }

    /// <inheritdoc />
    public override Department Add(Department department)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var id = connection.ExecuteScalar<int>(
            $@"
            INSERT INTO {this.TableName} (Name, Callsign, MaxUsers, UseGlobalRanks, CallsignFormat)
            VALUES (@Name, @Callsign, @MaxUsers, @UseGlobalRanks, @CallsignFormat);
            SELECT LAST_INSERT_ID();",
            new
            {
                Name = department.Name,
                Callsign = department.Callsign,
                MaxUsers = department.MaxUsers,
                UseGlobalRanks = department.UseGlobalRanks,
                CallsignFormat = department.CallsignFormat,
                DefaultRankId = department.DefaultRankId,
            });
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

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Name), update.Name);
        builder.Add(nameof(update.Callsign), update.Callsign);
        builder.Add(nameof(update.UseGlobalRanks), update.UseGlobalRanks);
        builder.Add(nameof(update.MaxUsers), update.MaxUsers);
        builder.Add(nameof(update.DefaultRankId), update.DefaultRankId);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, department.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }

    /// <summary>
    /// Attaches a rank to a department.
    /// </summary>
    /// <param name="department">The department.</param>
    /// <param name="rank">The rank.</param>
    public void AttachRank(Department department, Rank rank)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            INSERT INTO {TableNames.DepartmentRanks} (DepartmentId, RankId)
            VALUES (@DepartmentId, @RankId)
            ON DUPLICATE KEY UPDATE DepartmentId = @DepartmentId";
        connection.Execute(sql, new { DepartmentId = department.Id, RankId = rank.Id });
    }

    /// <summary>
    /// Removes a rank from a department.
    /// </summary>
    /// <param name="department">The department.</param>
    /// <param name="rank">The rank.</param>
    public void RemoveRank(Department department, Rank rank)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            DELETE FROM {TableNames.DepartmentRanks}
            WHERE DepartmentId = @DepartmentId AND RankId = @RankId";
        connection.Execute(sql, new { DepartmentId = department.Id, RankId = rank.Id });
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
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $@"
            SELECT r.*
            FROM {TableNames.DepartmentRanks} dr
            INNER JOIN {TableNames.Ranks} r ON dr.RankId = r.Id
            WHERE dr.DepartmentId = @DepartmentId";
        return connection.Query<Rank>(sql, new { DepartmentId = departmentId });
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
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $"SELECT * FROM {TableNames.Users} WHERE DepartmentId = @Id";
        return connection.Query<User>(sql, new { Id = departmentId });
    }
}