using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Repositories;

/// <summary>
/// Base repository class that provides common CRUD operations for all entities.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TId">The type of the entity's ID property.</typeparam>
public abstract class BaseRepository<T, TId>
    where T : class
{
#pragma warning disable
    protected readonly IDapperDatabaseConnectionFactory DbConnectionFactory;
    protected readonly string TableName;
    protected readonly string IdColumn = "Id";
#pragma warning restore

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRepository{T, TId}"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    /// <param name="tableName">The name of the database table.</param>
    protected BaseRepository(IDapperDatabaseConnectionFactory dbConnectionFactory, string tableName)
    {
        this.DbConnectionFactory = dbConnectionFactory;
        this.TableName = tableName;
    }

    /// <summary>
    /// Finds an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>The entity if found, null otherwise.</returns>
    public virtual T FindById(TId id)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var sql = $"SELECT * FROM {this.TableName} WHERE {this.IdColumn} = @Id";
        return connection.QueryFirstOrDefault<T>(sql, new { Id = id });
    }

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    public virtual void Delete(TId id)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var sql = $"DELETE FROM {this.TableName} WHERE {this.IdColumn} = @Id";
        connection.Execute(sql, new { Id = id });
    }

    /// <summary>
    /// Checks if an entity exists by its ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>True if the entity exists, false otherwise.</returns>
    public virtual bool Exists(TId id)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var sql = $"SELECT COUNT(*) FROM {this.TableName} WHERE {this.IdColumn} = @Id";
        return connection.ExecuteScalar<int>(sql, new { Id = id }) > 0;
    }

    /// <summary>
    /// Gets the count of all entities.
    /// </summary>
    /// <returns>The total count of entities.</returns>
    public virtual int Count()
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var sql = $"SELECT COUNT(*) FROM {this.TableName}";
        return connection.ExecuteScalar<int>(sql);
    }
}