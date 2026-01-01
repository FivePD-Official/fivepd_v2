using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for IncidentHistory CRUD operations.
/// </summary>
public class IncidentHistoryRepository : BaseRepository<IncidentHistory, IncidentHistoryUpdate, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentHistoryRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public IncidentHistoryRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.IncidentHistories)
    {
    }

    /// <inheritdoc />
    public override IncidentHistory Add(IncidentHistory incidentHistory)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                incidentHistory.IncidentId,
                incidentHistory.Title,
                incidentHistory.Description,
                incidentHistory.Location,
                incidentHistory.Priority,
                incidentHistory.Type,
                incidentHistory.GeneratedAt,
            })
            .Build();
        var id = connection.ExecuteScalar<string>(builderResult.Sql, builderResult.Parameters);
        incidentHistory.Id = id;
        return incidentHistory;
    }

    /// <inheritdoc />
    public override void Update(IncidentHistory incidentHistory, Action<IncidentHistoryUpdate> updateAction)
    {
        var update = new IncidentHistoryUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builderResult = SqlBuilder
            .Update(this.TableName)
            .SetTracked(update, this.IdColumn, incidentHistory.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Attaches a user to an incident history.
    /// </summary>
    /// <param name="incidentHistory">The incident history.</param>
    /// <param name="user">The user.</param>
    public void AttachUser(IncidentHistory incidentHistory, User user)
    {
        var builderResult = SqlBuilder
            .Into(TableNames.IncidentHistoryUsers)
            .Insert(new
            {
                IncidentHistoryId = incidentHistory.Id,
                UserId = user.Id,
            })
            .OnDuplicateKeyUpdate("IncidentHistoryId", "UserId")
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Removes a user from an incident history.
    /// </summary>
    /// <param name="incidentHistory">The incident history.</param>
    /// <param name="user">The user.</param>
    public void RemoveUser(IncidentHistory incidentHistory, User user)
    {
        var builderResult = SqlBuilder
            .From(TableNames.IncidentHistoryUsers)
            .WhereEquals("IncidentHistoryId", incidentHistory.Id)
            .WhereEquals("UserId", user.Id)
            .Delete()
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets all users attached to an incident history.
    /// </summary>
    /// <param name="incidentHistory">The incident history.</param>
    /// <returns>List of users.</returns>
    public IEnumerable<User> GetUsers(IncidentHistory incidentHistory)
    {
        var builderResult = SqlBuilder
            .From($"{TableNames.Users} u")
            .Select("u.*")
            .InnerJoin(TableNames.IncidentHistoryUsers, "ihu", "u.Id = ihu.UserId")
            .WhereEquals("ihu.IncidentHistoryId", incidentHistory.Id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<User>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Checks if a user is attached to an incident history.
    /// </summary>
    /// <param name="incidentHistory">The incident history.</param>
    /// <param name="user">The user.</param>
    /// <returns>True if the user is attached, false otherwise.</returns>
    public bool HasUser(IncidentHistory incidentHistory, User user)
    {
        var builderResult = SqlBuilder
            .From(TableNames.IncidentHistoryUsers)
            .WhereEquals("IncidentHistoryId", incidentHistory.Id)
            .WhereEquals("UserId", user.Id)
            .BuildCount();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters) > 0;
    }

    /// <summary>
    /// Retrieves all incident histories associated with a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of incident histories related to the specified user.</returns>
    public IEnumerable<IncidentHistory> GetAllOfUser(int userId)
    {
        var builderResult = SqlBuilder
            .From($"{TableNames.IncidentHistories} i")
            .Select("i.*")
            .LeftJoin(TableNames.IncidentHistoryUsers, "u", "u.IncidentHistoryId = i.Id")
            .WhereEquals("u.UserId", userId)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<IncidentHistory>(builderResult.Sql, builderResult.Parameters);
    }
}