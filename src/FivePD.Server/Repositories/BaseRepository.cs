using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.QueryHelpers;

namespace FivePD.Server.Repositories;

/// <summary>
/// Base repository class that provides common CRUD operations for all entities.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TEntityUpdate">The entity type for update operations.</typeparam>
/// <typeparam name="TId">The type of the entity's ID property.</typeparam>
public abstract class BaseRepository<TEntity, TEntityUpdate, TId>
    where TEntity : class
    where TEntityUpdate : ChangeTracker
{
#pragma warning disable
    protected readonly IDapperDatabaseConnectionFactory DbConnectionFactory;
    protected readonly string TableName;
    protected readonly string IdColumn = "Id";
#pragma warning restore
#pragma warning disable
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRepository{TEntity, TEntityUpdate, TId}"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    /// <param name="tableName">The name of the database table.</param>
    protected BaseRepository(IDapperDatabaseConnectionFactory dbConnectionFactory, string tableName)
    {
        this.DbConnectionFactory = dbConnectionFactory;
        this.TableName = tableName;
    }

    /// <summary>
    /// Adds a new entity to the repository and returns the added entity.
    /// </summary>
    /// <param name="entity">The entity to add to the repository.</param>
    /// <returns>The newly added entity with updated properties, such as its generated ID.</returns>
    public abstract TEntity Add(TEntity entity);

    /// <summary>
    /// Updates the specified entity using the provided update action.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <param name="updateAction">The action that applies the necessary updates.</param>
    public abstract void Update(TEntity entity, Action<TEntityUpdate> updateAction);

    /// <summary>
    /// Finds an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>The entity if found, null otherwise.</returns>
    public virtual TEntity FindById(TId id)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var sql = $"SELECT * FROM {this.TableName} WHERE {this.IdColumn} = @Id";
        return connection.QueryFirstOrDefault<TEntity>(sql, new { Id = id });
    }

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    public virtual void Delete(TId id)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .From(this.TableName)
            .Delete()
            .Where($"{this.IdColumn} = @Id", new { Id = id })
            .Build();
        connection.Execute(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Deletes multiple entities by their IDs.
    /// </summary>
    /// <param name="ids">The entity IDs to delete.</param>
    public virtual void DeleteRange(IEnumerable<TId> ids)
    {
        if (ids is null)
        {
            return;
        }

        var idList = ids.ToList();
        if (!idList.Any())
        {
            return;
        }

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .From(this.TableName)
            .Delete()
            .WhereIn(this.IdColumn, idList)
            .Build();
        connection.Execute(builderResult.Sql, builderResult.Parameters);
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

    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <returns>All entities.</returns>
    public virtual IEnumerable<TEntity> GetAll()
    {
        return this.GetAllWhere();
    }

    /// <summary>
    /// Gets all entities that satisfy the specified condition.
    /// </summary>
    /// <param name="condition">The SQL condition (without WHERE). If null or empty, returns all entities.</param>
    /// <param name="parameters">The parameters for the condition.</param>
    /// <returns>All entities matching the condition.</returns>
    public virtual IEnumerable<TEntity> GetAllWhere(string condition = null, object parameters = null)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var sql = $"SELECT * FROM {this.TableName}";
        if (!string.IsNullOrWhiteSpace(condition))
        {
            sql += $" WHERE {condition}";
        }

        return connection.Query<TEntity>(sql, parameters);
    }

    public virtual IEnumerable<T> Query<T>(string sql, object parameters = null)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<T>(sql, parameters);
    }

    /// <summary>
    /// Gets a single entity that satisfies the specified condition.
    /// </summary>
    /// <param name="condition">The SQL condition (without WHERE).</param>
    /// <param name="parameters">The parameters for the condition.</param>
    /// <returns>The entity if found, null otherwise.</returns>
    public virtual TEntity GetFirstOrDefault(string condition = null, object parameters = null)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var sql = $"SELECT * FROM {this.TableName}";
        if (!string.IsNullOrWhiteSpace(condition))
        {
            sql += $" WHERE {condition}";
        }

        sql += " LIMIT 1";

        return connection.QueryFirstOrDefault<TEntity>(sql, parameters);
    }

    /// <summary>
    /// Checks if any entity exists that satisfies the specified condition.
    /// </summary>
    /// <param name="condition">The SQL condition (without WHERE).</param>
    /// <param name="parameters">The parameters for the condition.</param>
    /// <returns>True if any entity exists, false otherwise.</returns>
    public virtual bool ExistsWhere(string condition = null, object parameters = null)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var sql = $"SELECT COUNT(*) FROM {this.TableName}";
        if (!string.IsNullOrWhiteSpace(condition))
        {
            sql += $" WHERE {condition}";
        }

        return connection.ExecuteScalar<int>(sql, parameters) > 0;
    }

    public virtual Page<TEntity> Paginate(int page, Action<ISelectBuilder> configure = null)
    {
        var builder = SqlBuilder.From(this.TableName);
        configure?.Invoke((ISelectBuilder)builder);

        using var connection = this.DbConnectionFactory.CreateScope().Connection;

        var countBuilderResult = builder.Build(isCount: true);
        var totalCount = connection.ExecuteScalar<int>(countBuilderResult.Sql, countBuilderResult.Parameters);

        var dataBuilderResult = ((SqlBuilder)builder).ApplyPagination(page).Build();
        var items = connection.Query<TEntity>(dataBuilderResult.Sql, dataBuilderResult.Parameters).ToList();

        return new Page<TEntity>
        {
            Items = items,
            CurrentPage = page,
            ItemCount = totalCount,
            PageSize = 10,
            PageCount = (int)Math.Ceiling((double)totalCount / 10),
        };
    }
}