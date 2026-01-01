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
        var builderResult = SqlBuilder
            .From(this.TableName)
            .WhereEquals(this.IdColumn, id)
            .Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.QueryFirstOrDefault<TEntity>(builderResult.Sql, builderResult.Parameters);
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
            .WhereEquals(this.IdColumn, id)
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
    /// Checks whether an entity with the specified identifier exists in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to check for existence.</param>
    /// <returns>True if the entity exists, otherwise false.</returns>
    public virtual bool Exists(TId id)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .From(this.TableName)
            .WhereEquals(this.IdColumn, id)
            .BuildCount();
        return connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters) > 0;
    }

    /// <summary>
    /// Determines whether any record exists in the database table based on the specified query builder configuration.
    /// </summary>
    /// <param name="queryBuilder">The query builder used to configure the query for checking existence.</param>
    /// <returns>True if at least one record exists based on the configured query, otherwise false.</returns>
    public virtual bool Exists(Action<IQueryBuilder> queryBuilder)
    {
        var builder = SqlBuilder.From(this.TableName);
        queryBuilder?.Invoke(builder);
        var builderResult = builder.BuildCount();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters) > 0;
    }

    /// <summary>
    /// Gets the count of all entities.
    /// </summary>
    /// <returns>The total count of entities.</returns>
    public virtual int Count()
    {
        var builderResult = SqlBuilder
            .From(this.TableName)
            .BuildCount();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <returns>All entities.</returns>
    public virtual IEnumerable<TEntity> GetAll(Action<IQueryBuilder> queryBuilder = null)
    {
        var builder = SqlBuilder.From(this.TableName);
        queryBuilder?.Invoke(builder);
        var builderResult = builder.Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<TEntity>(builderResult.Sql, builderResult.Parameters);
    }

    public virtual IEnumerable<T> Query<T>(string sql, object parameters = null)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<T>(sql, parameters);
    }

    public virtual Page<TEntity> Paginate(int page, Action<ISelectBuilder> configure = null)
    {
        var builder = SqlBuilder.From(this.TableName);
        configure?.Invoke((ISelectBuilder)builder);

        using var connection = this.DbConnectionFactory.CreateScope().Connection;

        var countBuilderResult = builder.BuildCount();
        var totalCount = connection.ExecuteScalar<int>(countBuilderResult.Sql, countBuilderResult.Parameters);

        var paginatedSqlBuilder = ((SqlBuilder)builder).ApplyPagination(page);
        var builderResult = ((ISelectBuilder)paginatedSqlBuilder).Build();
        var items = connection.Query<TEntity>(builderResult.Sql, builderResult.Parameters).ToList();

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