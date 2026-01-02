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
    protected readonly IDatabaseConnectionFactory DbConnectionFactory;
    protected readonly string TableName;
    protected readonly string IdColumn = "Id";
#pragma warning restore
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRepository{TEntity, TEntityUpdate, TId}"/> class.
    /// </summary>
    /// <param name="dbConnectionFactory">The database connection factory.</param>
    /// <param name="tableName">The name of the database table.</param>
    protected BaseRepository(IDatabaseConnectionFactory dbConnectionFactory, string tableName)
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
    /// Retrieves all entities from the database table represented by the repository.
    /// </summary>
    /// <param name="queryBuilder">An optional delegate to apply additional query conditions using the query builder.</param>
    /// <returns>A collection of entities of type <typeparamref name="TEntity"/>.</returns>
    public virtual IEnumerable<TEntity> GetAll(Action<IQueryBuilder> queryBuilder = null)
    {
        var builder = SqlBuilder.From(this.TableName);
        queryBuilder?.Invoke(builder);
        var builderResult = builder.Build();
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<TEntity>(builderResult.Sql, builderResult.Parameters);
    }

    /// <summary>
    /// Executes a SQL query and maps the result to a sequence of objects of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the objects to map the query results to.</typeparam>
    /// <param name="sql">The SQL query to be executed.</param>
    /// <param name="parameters">Optional parameters for the SQL query.</param>
    /// <returns>A sequence of objects of the specified type.</returns>
    public virtual IEnumerable<T> Query<T>(string sql, object parameters = null)
    {
        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        return connection.Query<T>(sql, parameters);
    }

    /// <summary>
    /// Retrieves a paginated list of entities from the database.
    /// </summary>
    /// <param name="page">The current page number.</param>
    /// <param name="configure">An optional action to configure the SQL query builder.</param>
    /// <returns>A <see cref="Page{T}"/> containing the paginated results, including items, total count, and pagination details.</returns>
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

    /// <summary>
    /// Executes a multi-mapping query and retrieves a list of primary entities by stitching
    /// related data from multiple result sets.
    /// </summary>
    /// <typeparam name="TPrimary">The type of the primary entity being queried.</typeparam>
    /// <typeparam name="TPrimaryId">The type of the identifier for the primary entity.</typeparam>
    /// <param name="buildResult">The result of building the SQL query, including SQL text, parameters, types, and split configuration.</param>
    /// <param name="stitcher">A function to stitch related data by mapping rows to the primary entity.</param>
    /// <param name="idSelector">A function to extract the identifier from the primary entity.</param>
    /// <returns>A list of the primary entity type with related data stitched in.</returns>
    public List<TPrimary> QueryMultiMap<TPrimary, TPrimaryId>(
        SqlBuilderResult buildResult,
        Func<TPrimary, RowReader, TPrimary> stitcher,
        Func<TPrimary, TPrimaryId> idSelector)
    {
        var lookup = new Dictionary<TPrimaryId, TPrimary>();

        using var connection = this.DbConnectionFactory.CreateScope().Connection;
        connection.Query(
            buildResult.Sql,
            buildResult.Types,
            objArray =>
            {
                var entity = (TPrimary)objArray[0];

                var id = idSelector(entity);
                if (!lookup.TryGetValue(id, out var current))
                {
                    current = entity;
                    lookup.Add(id, current);
                }

                var reader = new RowReader(objArray);
                return stitcher(current, reader);
            },
            buildResult.Parameters,
            splitOn: buildResult.SplitOn);

        return lookup.Values.ToList();
    }

    /// <summary>
    /// Provides functionality to read and process elements from a row of data.
    /// </summary>
    public record RowReader(object[] Row)
    {
        private int _ptr = 1;

        /// <summary>
        /// Reads and returns an element of type <typeparamref name="T"/> from the row if the specified condition is true.
        /// If the condition is false or no more elements are available, returns null.
        /// </summary>
        /// <typeparam name="T">The type of the element to be read.</typeparam>
        /// <param name="condition">A boolean value indicating whether to proceed with reading the element.</param>
        /// <returns>The element of type <typeparamref name="T"/> if the condition is true and the element exists; otherwise, null.</returns>
        public T Read<T>(bool condition)
            where T : class
        {
            if (!condition)
            {
                return null;
            }

            return this._ptr < this.Row.Length ? this.Row[this._ptr++] as T : null;
        }
    }
}