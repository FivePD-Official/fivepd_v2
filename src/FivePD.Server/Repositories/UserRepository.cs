using System;
using System.Linq;
using CitizenFX.Core;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for User CRUD operations.
/// </summary>
public class UserRepository : BaseRepository<User, UserUpdate, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public UserRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.Users)
    {
    }

    /// <inheritdoc />
    public override User Add(User user)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .Into(this.TableName)
            .Insert(new
            {
                user.Name,
                user.AccessFivePD,
                user.PreferredLanguage,
                user.FirstLogin,
                user.DismissedWelcomeMessage,
                user.IdentifierLicense,
                user.IdentifierDiscord,
                user.IdentifierSteam,
                user.IdentifierXbl,
                user.IdentifierLive,
                user.Xp,
                user.AudioVolume,
                user.Role,
                user.Callsign,
                user.UserCallsignFormatId,
                user.DepartmentId,
                user.DivisionId,
                user.RankId,
                user.FixedRank,
            })
            .Build();
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        user.Id = id;
        return user;
    }

    /// <inheritdoc />
    public override void Update(User user, Action<UserUpdate> updateAction)
    {
        var update = new UserUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.AccessFivePD), update.AccessFivePD);
        builder.Add(nameof(update.PreferredLanguage), update.PreferredLanguage);
        builder.Add(nameof(update.FirstLogin), update.FirstLogin);
        builder.Add(nameof(update.DismissedWelcomeMessage), update.DismissedWelcomeMessage);
        builder.Add(nameof(update.Name), update.Name);
        builder.Add(nameof(update.IdentifierLicense), update.IdentifierLicense);
        builder.Add(nameof(update.IdentifierDiscord), update.IdentifierDiscord);
        builder.Add(nameof(update.IdentifierSteam), update.IdentifierSteam);
        builder.Add(nameof(update.IdentifierXbl), update.IdentifierXbl);
        builder.Add(nameof(update.IdentifierLive), update.IdentifierLive);
        builder.Add(nameof(update.Xp), update.Xp);
        builder.Add(nameof(update.AudioVolume), update.AudioVolume);
        builder.Add(nameof(update.Role), update.Role);
        builder.Add(nameof(update.Callsign), update.Callsign);
        builder.Add(nameof(update.UserCallsignFormatId), update.UserCallsignFormatId);
        builder.Add(nameof(update.DepartmentId), update.DepartmentId);
        builder.Add(nameof(update.DivisionId), update.DivisionId);
        builder.Add(nameof(update.RankId), update.RankId);
        builder.Add(nameof(update.FixedRank), update.FixedRank);

        var (sql, parameters) = builder.Build(this.TableName, this.IdColumn, user.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }

    /// <summary>
    /// Finds a user by matching any identifier with the specified player's identifiers.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The user if found, null otherwise.</returns>
    public User FindByLicense(Player player)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder.From(this.TableName).WhereAnyLicenseMatches(player).Build();
        return connection.QueryFirstOrDefault<User>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Finds a user by their game license and dynamically includes related data based on the specified options.
    /// </summary>
    /// <param name="player">The player whose license should be used to find the user.</param>
    /// <param name="options">The options specifying which related user data to load.</param>
    /// <returns>The user associated with the specified license, including the related data as defined by the options.</returns>
    public User FindByLicenseWithDynamicRelated(Player player, UserLoadOptions options)
    {
        var builder = this.CreateBaseUserBuilder(options).WhereAnyLicenseMatches(player);
        return this.ExecuteUserQuery(builder, options);
    }

    /// <summary>
    /// Finds a user by its ID, including related entities based on the specified options.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="options">The options for loading related entities.</param>
    /// <returns>The user with requested related entities if found, null otherwise.</returns>
    public User FindByIdWithDynamicRelated(int id, UserLoadOptions options)
    {
        var builder = this.CreateBaseUserBuilder(options).Where("u.Id = @Id", new { Id = id });
        return this.ExecuteUserQuery(builder, options);
    }

    /// <summary>
    /// Adds a new UserCallsignFormat and updates the user's CallsignFormatId.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="callsignFormat">The callsign format to add.</param>
    /// <returns>The added UserCallsignFormat with its ID.</returns>
    public UserCallsignFormat AddCallsignFormat(User user, UserCallsignFormat callsignFormat)
    {
        var builderResult = SqlBuilder
            .Into(TableNames.UserCallsigns)
            .Insert(new { callsignFormat.Values })
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var id = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        callsignFormat.Id = id;
        return callsignFormat;
    }

    /// <summary>
    /// Updates an existing UserCallsignFormat.
    /// </summary>
    /// <param name="callsignFormat">The callsign format to update.</param>
    /// <param name="updateAction">The action that applies the necessary updates.</param>
    public void UpdateCallsignFormat(UserCallsignFormat callsignFormat, Action<UserCallsignFormatUpdate> updateAction)
    {
        var update = new UserCallsignFormatUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Values), update.Values);

        var (sql, parameters) = builder.Build(TableNames.UserCallsigns, this.IdColumn, callsignFormat.Id);
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(sql, parameters);
    }

    /// <summary>
    /// Deletes a UserCallsignFormat and removes it from the user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="callsignFormatId">The ID of the callsign format to delete.</param>
    public void DeleteCallsignFormat(User user, int callsignFormatId)
    {
        var builderResult = SqlBuilder
            .From(TableNames.UserCallsigns)
            .Delete()
            .WhereEquals("Id", callsignFormatId)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets the UserCallsignFormat of the specified user.
    /// </summary>
    /// <param name="user">The user whose callsign format to retrieve.</param>
    /// <returns>The UserCallsignFormat if found, null otherwise.</returns>
    public UserCallsignFormat GetCallsignFormat(User user)
    {
        var builderResult = SqlBuilder
            .From(TableNames.UserCallsigns)
            .WhereEquals("Id", user.UserCallsignFormatId)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.QueryFirstOrDefault<UserCallsignFormat>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Retrieves the department associated with the specified user.
    /// </summary>
    /// <param name="user">The user whose department is to be retrieved.</param>
    /// <returns>The department associated with the given user, or null if the user is null or no department is found.</returns>
    public Department GetDepartment(User user)
    {
        if (user is null)
        {
            return null;
        }

        var builderResult = SqlBuilder
            .From(TableNames.Departments)
            .WhereEquals("Id", user.DepartmentId)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.QueryFirstOrDefault<Department>(builderResult.Sql, builderResult.Parameters);
    }

    private User ExecuteUserQuery(IQueryBuilder builder, UserLoadOptions options)
    {
        var result = builder.Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query(
            result.Sql,
            result.Types,
            objArray =>
            {
                var user = (User)objArray[0];
                var index = 1;
                if (options.IncludeDepartment)
                {
                    user.Department = objArray[index++] as Department;
                }

                if (options.IncludeDivision)
                {
                    user.Division = objArray[index++] as Division;
                }

                if (options.IncludeRank)
                {
                    user.Rank = objArray[index++] as Rank;
                }

                if (options.IncludeCallsignFormat)
                {
                    user.CallsignFormat = objArray[index] as UserCallsignFormat;
                }

                return user;
            },
            result.Parameters,
            splitOn: result.SplitOn).FirstOrDefault();
    }

    private ISelectBuilder CreateBaseUserBuilder(UserLoadOptions options)
    {
        return SqlBuilder
            .From($"{TableNames.Users} u")
            .Select("u.*", typeof(User))
            .If(options.IncludeDepartment, b => b.Select("d.*", typeof(Department)).LeftJoin(TableNames.Departments, "d", "u.DepartmentId = d.Id"))
            .If(options.IncludeDivision, b => b.Select("di.*", typeof(Division)).LeftJoin(TableNames.Divisions, "di", "u.DivisionId = di.Id"))
            .If(options.IncludeRank, b => b.Select("r.*", typeof(Rank)).LeftJoin(TableNames.Ranks, "r", "u.RankId = r.Id"))
            .If(
                options.IncludeCallsignFormat,
                b => b.Select("cf.*", typeof(UserCallsignFormat)).LeftJoin(TableNames.UserCallsigns, "cf", "u.UserCallsignFormatId = cf.Id"));
    }

#pragma warning disable
    public class UserLoadOptions
    {
        public bool IncludeDepartment { get; set; }
        public bool IncludeDivision { get; set; }
        public bool IncludeRank { get; set; }
        public bool IncludeCallsignFormat { get; set; }
    }
#pragma warning restore
}