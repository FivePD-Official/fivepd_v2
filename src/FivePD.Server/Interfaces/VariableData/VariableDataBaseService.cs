using System;
using System.Collections.Generic;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models.VariableData;

namespace FivePD.Server.Interfaces.VariableData;

/// <summary>
/// Responsible for managing loose data set entities.
/// </summary>
/// <typeparam name="TS">The type of the set to use.</typeparam>
public abstract class VariableDataBaseService<TS> : IOnInit, IOnDestroy
    where TS : VariableDataSet
{
    private readonly IVariableDataService _variableDataService;

    private readonly List<IDisposable> _disposables = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableDataBaseService{TS}"/> class.
    /// </summary>
    /// <param name="variableDataService">The <see cref="IVariableDataService"/> to use.</param>
    protected VariableDataBaseService(IVariableDataService variableDataService)
    {
        this._variableDataService = variableDataService;
    }

    /// <summary>
    /// Invoked when an entity is added.
    /// </summary>
    public event EventHandler<VariableDataEvent> OnAdded;

    /// <summary>
    /// Invoked when an entity is updated.
    /// </summary>
    public event EventHandler<VariableDataEvent> OnUpdated;

    /// <summary>
    /// Invoked when an entity is deleted.
    /// </summary>
    public event EventHandler<VariableDataEvent> OnDeleted;

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._variableDataService.RegisterSetAttributes<TS>(this.RegisterSetAttributes());
        this._disposables.Add(this._variableDataService.OnAdded<TS>(@event => this.OnAdded?.Invoke(this, @event)));
        this._disposables.Add(this._variableDataService.OnUpdated<TS>(@event => this.OnUpdated?.Invoke(this, @event)));
        this._disposables.Add(this._variableDataService.OnDeleted<TS>(@event => this.OnDeleted?.Invoke(this, @event)));
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
    public VariableDataEntity Add(VariableDataEntity entity)
    {
        return this._variableDataService.AddEntity<TS>(entity);
    }

    /// <summary>
    /// Updates an entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    public void Update(VariableDataEntity entity)
    {
        this._variableDataService.UpdateEntity(entity);
    }

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public void Delete(VariableDataEntity entity)
    {
        this._variableDataService.DeleteEntity(entity);
    }

    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to get.</param>
    /// <returns>The entity.</returns>
    public VariableDataEntity Get(string id)
    {
        return this._variableDataService.GetEntity(id);
    }

    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to get.</param>
    /// <param name="queryCoreAttributeValue">Whether to query the core attribute values or not.</param>
    /// <returns>The entity.</returns>
    public VariableDataEntity Get(string id, bool queryCoreAttributeValue)
    {
        return this._variableDataService.GetEntity(id, queryCoreAttributeValue);
    }

    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <param name="queryCoreAttributeValue">The id sof the entity.</param>
    /// <param name="filter">The id of the entity.</param>
    /// <returns>The entities.</returns>
    public List<VariableDataEntity> GetAll(bool queryCoreAttributeValue, Dictionary<string, object> filter = null)
    {
        return this._variableDataService.GetEntities<TS>(queryCoreAttributeValue, filter);
    }

    /// <summary>
    /// Registers the attributes for the set.
    /// </summary>
    /// <returns>The attributes.</returns>
    protected abstract List<VariableDataAttribute> RegisterSetAttributes();
}