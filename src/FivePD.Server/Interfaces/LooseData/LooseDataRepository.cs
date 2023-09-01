using System;
using System.Collections.Generic;
using FivePD.Server.Models.LooseData;

namespace FivePD.Server.Interfaces.LooseData;

#pragma warning disable

/// <summary>
/// Responsible for managing loose data set entities.
/// </summary>
/// <typeparam name="TS">The type of the set to use.</typeparam>
public abstract class LooseDataRepository<TS> : IServiceWithEvents
    where TS : LooseDataSet
{
    private readonly ILooseDataService _looseDataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LooseDataRepository{TS}"/> class.
    /// </summary>
    /// <param name="looseDataService">The <see cref="ILooseDataService"/> to use.</param>
    protected LooseDataRepository(ILooseDataService looseDataService)
    {
        this._looseDataService = looseDataService;
    }
    
    void IServiceWithEvents.RegisterEvents()
    {
        this._looseDataService.RegisterSetAttributes<TS>(this.RegisterSetAttributes());
    }

    public IDisposable OnAdded(Action<LooseDataEntity> observer)
    {
        return this._looseDataService.OnAdded<TS>(observer);
    }

    public IDisposable OnUpdated(Action<LooseDataEntity> observer)
    {
        return this._looseDataService.OnUpdated<TS>(observer);
    }

    public IDisposable OnDeleted(Action<LooseDataEntity> observer)
    {
        return this._looseDataService.OnDeleted<TS>(observer);
    }

    public void Add(LooseDataEntity arrestReport)
    {
        this._looseDataService.AddEntity<TS>(arrestReport);
    }

    public void Update(LooseDataEntity arrestReport)
    {
        this._looseDataService.UpdateEntity(arrestReport);
    }

    public void Delete(LooseDataEntity arrestReport)
    {
        this._looseDataService.DeleteEntity(arrestReport);
    }

    public LooseDataEntity Get(string id)
    {
        return this._looseDataService.GetEntity(id);
    }

    public List<LooseDataEntity> GetAll()
    {
        return this._looseDataService.GetEntities<TS>();
    }

    protected abstract List<LooseDataAttribute> RegisterSetAttributes();
}