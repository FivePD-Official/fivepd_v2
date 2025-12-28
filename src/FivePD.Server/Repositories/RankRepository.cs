using Dapper;
using FivePD.Common;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for Rank CRUD operations.
/// </summary>
public class RankRepository : BaseRepository<Rank, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RankRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public RankRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.Ranks)
    {
    }

    /// <summary>
    /// Adds a new rank to the database.
    /// </summary>
    /// <param name="rank">The rank entity to be added.</param>
    /// <returns>The ID of the newly created rank.</returns>
    public int Add(Rank rank)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var id = connection.ExecuteScalar<int>(
            $@"
            INSERT INTO {this.TableName} (Global, RequiredXp, Name, Permission)
            VALUES (@Global, @RequiredXp, @Name, @Permission);
            SELECT LAST_INSERT_ID();",
            new
            {
                Global = rank.Global,
                RequiredXp = rank.RequiredXp,
                Name = rank.Name,
                Permission = rank.Permission,
            });
        return id;
    }
}