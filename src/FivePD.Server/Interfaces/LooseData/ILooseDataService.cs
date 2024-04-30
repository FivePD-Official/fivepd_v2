using System;
using System.Collections.Generic;
using FivePD.Server.Models.LooseData;

namespace FivePD.Server.Interfaces.LooseData;

/// <summary>
/// Handles the dynamic data sets and entities.
/// </summary>
public interface ILooseDataService
{
    /// <summary>
    /// Registers the given <paramref name="repository"/>.
    /// </summary>
    /// <param name="repository">The repository to register.</param>
    /// <typeparam name="TS">The type of the set to register.</typeparam>
    /// <typeparam name="TR">The type of the repository to register.</typeparam>
    public void RegisterAddonRepository<TS, TR>(TR repository)
        where TS : LooseDataSet
        where TR : LooseDataRepository<TS>;

    /// <summary>
    /// Registers attributes to the given dynamic data set type.
    /// </summary>
    /// <param name="attributes">The attributes to register.</param>
    /// <typeparam name="TS">The type of the set.</typeparam>
    public void RegisterSetAttributes<TS>(List<LooseDataAttribute> attributes)
        where TS : LooseDataSet;

    /// <summary>
    /// Updates the attribute of the given name
    /// with the values of <paramref name="newAttributeValues"/>.
    /// Can't update another addon's attribute or change its data type.
    /// </summary>
    /// <param name="attributeName">The attribute's name to delete.</param>
    /// <param name="newAttributeValues">The new values of the attribute.</param>
    /// <typeparam name="TS">The set to delete the attribute from.</typeparam>
    /// <exception cref="ArgumentException">When the executing addon tries to delete another addon's attribute.</exception>
    public void UpdateSetAttribute<TS>(string attributeName, LooseDataAttribute newAttributeValues)
        where TS : LooseDataSet;

    /// <summary>
    /// Deletes an attribute with the given name
    /// from the <typeparamref name="TS"></typeparamref> set.
    /// Can't delete another addon's attribute.
    /// </summary>
    /// <param name="attributeName">The attribute's name to delete.</param>
    /// <typeparam name="TS">The set to delete the attribute from.</typeparam>
    /// <exception cref="ArgumentException">When the executing addon tries to delete another addon's attribute.</exception>
    public void DeleteSetAttribute<TS>(string attributeName)
        where TS : LooseDataSet;

    /// <summary>
    /// Fires when an entity is added to the given <typeparamref name="TS"/> set
    /// either via the API or the NUI.
    /// </summary>
    /// <param name="observer">The action that will executed upon firing with the added entity.</param>
    /// <typeparam name="TS">The type of the set to subscribe to.</typeparam>
    /// <returns>A disposable <see cref="LooseDataEntityObservableSubscription"/>.</returns>
    public IDisposable OnAdded<TS>(Action<LooseDataEvent> observer);

    /// <summary>
    /// Fires when an entity in the given <typeparamref name="TS"/> set is edited
    /// either via the API or the NUI.
    /// </summary>
    /// <param name="observer">The action that will executed upon firing with the edited entity.</param>
    /// <typeparam name="TS">The type of the set to subscribe to.</typeparam>
    /// <returns>A disposable <see cref="LooseDataEntityObservableSubscription"/>.</returns>
    public IDisposable OnUpdated<TS>(Action<LooseDataEvent> observer);

    /// <summary>
    /// Fires when an entity from the given <typeparamref name="TS"/> set is deleted
    /// either via the API or the NUI.
    /// </summary>
    /// <param name="observer">The action that will executed upon fire deleted entity.</param>
    /// <typeparam name="TS">The type of the set to subscribe to.</typeparam>
    /// <returns>A disposable <see cref="LooseDataEntityObservableSubscription"/>.</returns>
    public IDisposable OnDeleted<TS>(Action<LooseDataEvent> observer);

    /// <summary>
    /// Inserts an entity into the database with all its
    /// values mapped to the attributes of the given set.
    /// If a property doesn't exist as an attribute it won't be saved.
    /// </summary>
    /// <param name="entityToAdd">The entity to insert.</param>
    /// <typeparam name="TS">The type of the set where the entity should be inserted.</typeparam>
    /// <returns>The inserted entity.</returns>
    public LooseDataEntity AddEntity<TS>(LooseDataEntity entityToAdd)
        where TS : LooseDataSet;

    /// <summary>
    /// Updates the values of the entity in the database.
    /// </summary>
    /// <param name="entityToUpdate">The entity to update.</param>
    public void UpdateEntity(LooseDataEntity entityToUpdate);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entityToDelete">The entity to delete.</param>
    public void DeleteEntity(LooseDataEntity entityToDelete);

    /// <summary>
    /// Finds and maps the first entity with the given id.
    /// </summary>
    /// <param name="id">The id of the entity.</param>
    /// <returns>The first entity with the given id.</returns>
    public LooseDataEntity GetEntity(string id);

    /// <summary>
    /// Finds and maps the first entity with the given id.
    /// </summary>
    /// <param name="id">The id of the entity.</param>
    /// <param name="withTracking">Whether to track the entities or not.</param>
    /// <param name="queryCoreAttributeValue">Whether to query the core attribute values or not.</param>
    /// <returns>The first entity with the given id.</returns>
    public LooseDataEntity GetEntity(string id, bool withTracking, bool queryCoreAttributeValue);

    /// <summary>
    /// Finds and maps all entities of the given set.
    /// </summary>
    /// <typeparam name="TS">The type of the set which entities are needed.</typeparam>
    /// <param name="withTracking">Whether to track the entities or not.</param>
    /// <param name="queryCoreAttributeValue">Whether to query the core attribute values or not.</param>
    /// <param name="filter">The filter to apply to the query.</param>
    /// <returns>All entities of the set.</returns>
    public List<LooseDataEntity> GetEntities<TS>(bool withTracking, bool queryCoreAttributeValue, Dictionary<string, object> filter = null)
        where TS : LooseDataSet;

    /// <summary>
    /// Finds and maps all entities of the given set.
    /// </summary>
    /// <param name="setType">The id of the set.</param>
    /// <param name="withTracking">Whether to track the entities or not.</param>
    /// <param name="queryCoreAttributeValue">Whether to query the core attribute values or not.</param>
    /// <param name="filter">The filter to apply to the query.</param>
    /// <returns>All entities of the set.</returns>
    public List<LooseDataEntity> GetEntities(Type setType, bool withTracking, bool queryCoreAttributeValue, Dictionary<string, object> filter = null);
}

/// <summary>
/// Handles the dynamic data sets and entities.
/// </summary>
internal interface IInternalLooseDataService
{
    /// <summary>
    /// Disposes all addon repositories.
    /// </summary>
    public void DisposeAddonRepositories();
}