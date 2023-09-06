using System;
using System.Collections.Generic;
using FivePD.Server.Models.LooseData;

namespace FivePD.Server.Interfaces.LooseData;

/// <summary>
/// Responsible for managing loose data set entities.
/// </summary>
/// <typeparam name="TS">The type of the set to use.</typeparam>
public abstract class LooseDataRepository<TS> : IServiceWithEvents, IDisposableService
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
    void IServiceWithEvents.RegisterEvents()
    {
        this._looseDataService.RegisterSetAttributes<TS>(this.RegisterSetAttributes());
        this._disposables.Add(this._looseDataService.OnAdded<TS>(entity => this.OnAdded?.Invoke(this, entity)));
        this._disposables.Add(this._looseDataService.OnUpdated<TS>(entity => this.OnUpdated?.Invoke(this, entity)));
        this._disposables.Add(this._looseDataService.OnDeleted<TS>(entity => this.OnDeleted?.Invoke(this, entity)));
    }

    /// <inheritdoc/>
    void IDisposableService.InternalDispose()
    {
        foreach (var disposable in this._disposables)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="arrestReport">The entity to add.</param>
    public void Add(LooseDataEntity arrestReport)
    {
        this._looseDataService.AddEntity<TS>(arrestReport);
    }

    /// <summary>
    /// Updates an entity.
    /// </summary>
    /// <param name="arrestReport">The entity to update.</param>
    public void Update(LooseDataEntity arrestReport)
    {
        this._looseDataService.UpdateEntity(arrestReport);
    }

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="arrestReport">The entity to delete.</param>
    public void Delete(LooseDataEntity arrestReport)
    {
        this._looseDataService.DeleteEntity(arrestReport);
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
    /// Gets all entities.
    /// </summary>
    /// <returns>The entities.</returns>
    public List<LooseDataEntity> GetAll()
    {
        return this._looseDataService.GetEntities<TS>();
    }

    /// <summary>
    /// Registers the attributes for the set.
    /// </summary>
    /// <returns>The attributes.</returns>
    protected abstract List<LooseDataAttribute> RegisterSetAttributes();
}