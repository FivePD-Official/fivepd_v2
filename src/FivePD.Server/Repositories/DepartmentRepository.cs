using Dapper;
using FivePD.Common;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Repositories;

/// <summary>
/// Repository for Department CRUD operations.
/// </summary>
public class DepartmentRepository : BaseRepository<Department, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DepartmentRepository"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    public DepartmentRepository(IDapperDatabaseConnectionFactory dbConnectionFactory)
        : base(dbConnectionFactory, TableNames.Departments)
    {
    }

    /// <summary>
    /// Adds a new department to the database.
    /// </summary>
    /// <param name="department">The department entity to be added.</param>
    /// <returns>The ID of the newly created department.</returns>
    public int Add(Department department)
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
        return id;
    }
}