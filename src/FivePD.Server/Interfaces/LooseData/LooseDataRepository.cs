using System;
using System.Collections.Generic;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models.LooseData;

namespace FivePD.Server.Interfaces.LooseData;

/// <summary>
/// Responsible for managing loose data set entities.
/// </summary>
/// <typeparam name="TS">The type of the set to use.</typeparam>
public abstract class LooseDataRepository<TS> : IOnInit, IOnDestroy
    where TS : LooseDataSet
{
    private readonly ILooseDataService _looseDataService;

    private readonly List<IDisposable> _disposables = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LooseDataRepository{TS}"/> class.
    /// </summary>
    /// <param name="looseDataService">The <see cref="ILooseDataService"/> to use.</param>
    protected LooseDataRepository(ILooseDataService looseDataService)
    {
        this._looseDataService = looseDataService;
    }

    /// <summary>
    /// Invoked when an entity is added.
    /// </summary>
    public event EventHandler<LooseDataEntity> OnAdded;

    /// <summary>
    /// Invoked when an entity is updated.
    /// </summary>
    public event EventHandler<LooseDataEntity> OnUpdated;

    /// <summary>
    /// Invoked when an entity is deleted.
    /// </summary>
    public event EventHandler<LooseDataEntity> OnDeleted;

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._looseDataService.RegisterSetAttributes<TS>(this.RegisterSetAttributes());
        this._disposables.Add(this._looseDataService.OnAdded<TS>(entity => this.OnAdded?.Invoke(this, entity)));
        this._disposables.Add(this._looseDataService.OnUpdated<TS>(entity => this.OnUpdated?.Invoke(this, entity)));
        this._disposables.Add(this._looseDataService.OnDeleted<TS>(entity => this.OnDeleted?.Invoke(this, entity)));
    }

    /// <inheritdoc/>
    void IOnDestroy.OnDestroy()
    {
        foreach (var disposable in this._disposables)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The inserted entity.</returns>
    public LooseDataEntity Add(LooseDataEntity entity)
    {
        return this._looseDataService.AddEntity<TS>(entity);
    }

    /// <summary>
    /// Updates an entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public void Update(LooseDataEntity entity)
    {
        this._looseDataService.UpdateEntity(entity);
    }

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public void Delete(LooseDataEntity entity)
    {
        this._looseDataService.DeleteEntity(entity);
    }

    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to get.</param>
    /// <returns>The entity.</returns>
    public LooseDataEntity Get(string id)
    {
        return this._looseDataService.GetEntity(id);
    }

    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to get.</param>
    /// <param name="withTracking">Whether to track the entities or not.</param>
    /// <param name="queryCoreAttributeValue">Whether to query the core attribute values or not.</param>
    /// <returns>The entity.</returns>
    public LooseDataEntity Get(string id, bool withTracking, bool queryCoreAttributeValue)
    {
        return this._looseDataService.GetEntity(id, withTracking, queryCoreAttributeValue);
    }

    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <param name="withTracking">The id sof the withTracking.</param>
    /// <param name="queryCoreAttributeValue">The id sof the entity.</param>
    /// <param name="filter">The id of the entity.</param>
    /// <returns>The entities.</returns>
    public List<LooseDataEntity> GetAll(bool withTracking, bool queryCoreAttributeValue, Dictionary<string, object> filter = null)
    {
        return this._looseDataService.GetEntities<TS>(withTracking, queryCoreAttributeValue, filter);
    }

    /// <summary>
    /// Registers the attributes for the set.
    /// </summary>
    /// <returns>The attributes.</returns>
    protected abstract List<LooseDataAttribute> RegisterSetAttributes();
}