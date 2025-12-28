using Dapper;
using FivePD.Common;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for Division CRUD operations.
/// </summary>
public class DivisionRepository : BaseRepository<Division, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DivisionRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public DivisionRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.Divisions)
    {
    }

    /// <summary>
    /// Adds a new division to the database.
    /// </summary>
    /// <param name="division">The division entity to be added.</param>
    /// <returns>The ID of the newly created division.</returns>
    public int Add(Division division)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var id = connection.ExecuteScalar<int>(
            $@"
            INSERT INTO {this.TableName} (Name, DepartmentId)
            VALUES (@Name, @DepartmentId);
            SELECT LAST_INSERT_ID();",
            new
            {
                Name = division.Name,
                DepartmentId = division.DepartmentId,
            });
        return id;
    }
}