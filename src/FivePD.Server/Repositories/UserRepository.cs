using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.UpdateModels;

namespace FivePD.Server.Repositories;

#pragma warning disable

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
        var id = connection.ExecuteScalar<int>(
            $@"INSERT INTO {this.TableName} (Name, AccessFivePD, PreferredLanguage, FirstLogin, DismissedWelcomeMessage, IdentifierLicense, IdentifierDiscord, IdentifierSteam, IdentifierXbl, IdentifierLive, Xp, AudioVolume, Role, Callsign, UserCallsignFormatId, DepartmentId, DivisionId, RankId, FixedRank)
            VALUES (@Name, @AccessFivePD, @PreferredLanguage, @FirstLogin, @DismissedWelcomeMessage, @IdentifierLicense, @IdentifierDiscord, @IdentifierSteam, @IdentifierXbl, @IdentifierLive, @Xp, @AudioVolume, @Role, @Callsign, @UserCallsignFormatId, @DepartmentId, @DivisionId, @RankId, @FixedRank);
            SELECT LAST_INSERT_ID();",
            new
            {
                Name = user.Name,
                AccessFivePD = user.AccessFivePD,
                PreferredLanguage = user.PreferredLanguage,
                FirstLogin = user.FirstLogin,
                DismissedWelcomeMessage = user.DismissedWelcomeMessage,
                IdentifierLicense = user.IdentifierLicense,
                IdentifierDiscord = user.IdentifierDiscord,
                IdentifierSteam = user.IdentifierSteam,
                IdentifierXbl = user.IdentifierXbl,
                IdentifierLive = user.IdentifierLive,
                Xp = user.Xp,
                AudioVolume = user.AudioVolume,
                Role = user.Role,
                Callsign = user.Callsign,
                UserCallsignFormatId = user.UserCallsignFormatId,
                DepartmentId = user.DepartmentId,
                DivisionId = user.DivisionId,
                RankId = user.RankId,
                FixedRank = user.FixedRank,
            });
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
        var parameters = GetPlayerIdentifiers(player);

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var sql = $@"SELECT * FROM {this.TableName}
            WHERE IdentifierLicense = @License
               OR IdentifierDiscord = @Discord
               OR IdentifierSteam = @Steam
               OR IdentifierXbl = @Xbl
               OR IdentifierLive = @Live
            LIMIT 1";
        return connection.QueryFirstOrDefault<User>(sql, parameters);
    }

    /// <summary>
    /// Finds a user by matching any identifier with the specified player's identifiers,
    /// including related entities (Department, Division, Rank, CallsignFormat).
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The user with related entities if found, null otherwise.</returns>
    public User FindByLicenseWithRelated(Player player)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var parameters = GetPlayerIdentifiers(player);
        var sql = $@"SELECT u.*, d.*, div.*, r.*, cf.*
            FROM {this.TableName} u
            LEFT JOIN {TableNames.Departments} d ON u.DepartmentId = d.Id
            LEFT JOIN {TableNames.Divisions} div ON u.DivisionId = div.Id
            LEFT JOIN {TableNames.Ranks} r ON u.RankId = r.Id
            LEFT JOIN {TableNames.UserCallsigns} cf ON u.UserCallsignFormatId = cf.Id
            WHERE u.IdentifierLicense = @License
               OR u.IdentifierDiscord = @Discord
               OR u.IdentifierSteam = @Steam
               OR u.IdentifierXbl = @Xbl
               OR u.IdentifierLive = @Live
            LIMIT 1";

        return connection.Query<User, Department, Division, Rank, UserCallsignFormat, User>(
            sql,
            (user, department, division, rank, callsignFormat) =>
            {
                user.Department = department;
                user.Division = division;
                user.Rank = rank;
                user.CallsignFormat = callsignFormat;
                return user;
            },
            parameters,
            splitOn: "Id,Id,Id,Id").FirstOrDefault();
    }

    /// <summary>
    /// Finds a user by its ID, including related entities (CallsignFormat, Department and its Users).
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user with related entities if found, null otherwise.</returns>
    public User FindByIdWithRelated(int id)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var sql = $@"SELECT u.*, cf.*, d.*, du.*
            FROM {this.TableName} u
            LEFT JOIN {TableNames.UserCallsigns} cf ON u.UserCallsignFormatId = cf.Id
            LEFT JOIN {TableNames.Departments} d ON u.DepartmentId = d.Id
            LEFT JOIN {this.TableName} du ON d.Id = du.DepartmentId
            WHERE u.{this.IdColumn} = @Id";

        User user = null;
        connection.Query<User, UserCallsignFormat, Department, User, User>(
            sql,
            (u, cf, d, du) =>
            {
                if (user == null)
                {
                    user = u;
                    user.CallsignFormat = cf;
                    user.Department = d;
                    if (user.Department != null)
                    {
                        user.Department.Users = new List<User>();
                    }
                }

                if (du != null && user.Department != null)
                {
                    user.Department.Users.Add(du);
                }

                return u;
            },
            new { Id = id },
            splitOn: "Id,Id,Id");

        return user;
    }

    /// <summary>
    /// Options for loading related entities for a user.
    /// </summary>
    public class UserLoadOptions
    {
        public bool IncludeDepartment { get; set; }
        public bool IncludeDivision { get; set; }
        public bool IncludeRank { get; set; }
        public bool IncludeCallsignFormat { get; set; }
    }

    /// <summary>
    /// Finds a user by its ID, including related entities based on the specified options.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="options">The options for loading related entities.</param>
    /// <returns>The user with requested related entities if found, null otherwise.</returns>
    public User FindByIdWithDynamicRelated(int id, UserLoadOptions options)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var selects = new List<string> { "u.*" };
        var joins = new List<string>();
        var types = new List<Type> { typeof(User) };
        var splitOn = new List<string>();

        if (options.IncludeDepartment)
        {
            selects.Add("d.*");
            joins.Add($"LEFT JOIN {TableNames.Departments} d ON u.DepartmentId = d.Id");
            types.Add(typeof(Department));
            splitOn.Add("Id");
        }

        if (options.IncludeDivision)
        {
            selects.Add("div.*");
            joins.Add($"LEFT JOIN {TableNames.Divisions} div ON u.DivisionId = div.Id");
            types.Add(typeof(Division));
            splitOn.Add("Id");
        }

        if (options.IncludeRank)
        {
            selects.Add("r.*");
            joins.Add($"LEFT JOIN {TableNames.Ranks} r ON u.RankId = r.Id");
            types.Add(typeof(Rank));
            splitOn.Add("Id");
        }

        if (options.IncludeCallsignFormat)
        {
            selects.Add("cf.*");
            joins.Add($"LEFT JOIN {TableNames.UserCallsigns} cf ON u.UserCallsignFormatId = cf.Id");
            types.Add(typeof(UserCallsignFormat));
            splitOn.Add("Id");
        }

        var sql = $@"SELECT {string.Join(", ", selects)}
            FROM {this.TableName} u
            {string.Join("\n", joins)}
            WHERE u.{this.IdColumn} = @Id
            LIMIT 1";

        return connection.Query(
            sql,
            types.ToArray(),
            objArray =>
            {
                var user = (User)objArray[0];
                var index = 1;
                if (options.IncludeDepartment)
                {
                    user.Department = (Department)objArray[index++];
                }

                if (options.IncludeDivision)
                {
                    user.Division = (Division)objArray[index++];
                }

                if (options.IncludeRank)
                {
                    user.Rank = (Rank)objArray[index++];
                }

                if (options.IncludeCallsignFormat)
                {
                    user.CallsignFormat = (UserCallsignFormat)objArray[index];
                }

                return user;
            },
            new { Id = id },
            splitOn: splitOn.Count > 0 ? string.Join(",", splitOn) : "Id").FirstOrDefault();
    }

    /// <summary>
    /// Adds a new UserCallsignFormat and updates the user's CallsignFormatId.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="callsignFormat">The callsign format to add.</param>
    /// <returns>The added UserCallsignFormat with its ID.</returns>
    public UserCallsignFormat AddCallsignFormat(User user, UserCallsignFormat callsignFormat)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $"INSERT INTO {TableNames.UserCallsigns} (Values) VALUES (@Values); SELECT LAST_INSERT_ID();";
        var id = connection.ExecuteScalar<int>(sql, new { Values = string.Join(",", callsignFormat.Values) });
        callsignFormat.Id = id;
        return callsignFormat;
    }

    /// <summary>
    /// Updates an existing UserCallsignFormat.
    /// </summary>
    /// <param name="callsignFormatId">The ID of the callsign format to update.</param>
    /// <param name="updateAction">The action that applies the necessary updates.</param>
    public void UpdateCallsignFormat(UserCallsignFormat callsignFormatId, Action<UserCallsignFormatUpdate> updateAction)
    {
        var update = new UserCallsignFormatUpdate();
        updateAction(update);
        if (!update.HasChanges())
        {
            return;
        }

        var builder = new UpdateQueryBuilder(update);
        builder.Add(nameof(update.Values), update.Values);

        var (sql, parameters) = builder.Build(TableNames.UserCallsigns, this.IdColumn, callsignFormatId);
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
        using var connection = this.DbConnectionFactory.CreateScope().Connection;

        var updateSql = $"UPDATE {this.TableName} SET UserCallsignFormatId = NULL WHERE {this.IdColumn} = @UserId";
        connection.Execute(updateSql, new { UserId = user.Id });

        const string deleteSql = $"DELETE FROM {TableNames.UserCallsigns} WHERE Id = @Id";
        connection.Execute(deleteSql, new { Id = callsignFormatId });
    }

    /// <summary>
    /// Gets the UserCallsignFormat of the specified user.
    /// </summary>
    /// <param name="user">The user whose callsign format to retrieve.</param>
    /// <returns>The UserCallsignFormat if found, null otherwise.</returns>
    public UserCallsignFormat GetCallsignFormat(User user)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        const string sql = $"SELECT * FROM {TableNames.UserCallsigns} WHERE Id = @Id";
        return connection.QueryFirstOrDefault<UserCallsignFormat>(sql, new { Id = user.UserCallsignFormatId });
    }

    private object GetPlayerIdentifiers(Player player)
    {
        return new
        {
            License = player.Identifiers["license"] ?? string.Empty,
            Discord = player.Identifiers["discord"] ?? string.Empty,
            Steam = player.Identifiers["steam"] ?? string.Empty,
            Xbl = player.Identifiers["xbl"] ?? string.Empty,
            Live = player.Identifiers["live"] ?? string.Empty,
        };
    }
}