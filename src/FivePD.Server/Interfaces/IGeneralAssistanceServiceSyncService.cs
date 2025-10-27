using System;
using System.Collections.Generic;
using CitizenFX.Core;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for synchronizing entities between assistance service instances to prevent duplicate work.
/// </summary>
public interface IGeneralAssistanceServiceSyncService
{
    /// <summary>
    /// Adds an entity to the sync service for a specific assistance service type and location.
    /// </summary>
    /// <param name="assistanceServiceType">The type of assistance service.</param>
    /// <param name="entity">The entity to add.</param>
    /// <param name="serviceLocation">The location of the assistance service instance.</param>
    /// <param name="maxRadius">The maximum radius for this service instance (optional, defaults to 200).</param>
    void AddEntity(Type assistanceServiceType, Entity entity, Vector3 serviceLocation, float maxRadius = 200f);

    /// <summary>
    /// Adds multiple entities to the sync service for a specific assistance service type and location.
    /// </summary>
    /// <param name="assistanceServiceType">The type of assistance service.</param>
    /// <param name="entities">The entities to add.</param>
    /// <param name="serviceLocation">The location of the assistance service instance.</param>
    /// <param name="maxRadius">The maximum radius for this service instance (optional, defaults to 200).</param>
    void AddEntities(Type assistanceServiceType, IEnumerable<Entity> entities, Vector3 serviceLocation, float maxRadius = 200f);

    /// <summary>
    /// Marks an entity as having work started on it.
    /// </summary>
    /// <param name="entity">The entity to mark as work started.</param>
    /// <returns>True if the entity was successfully marked, false if it doesn't exist or work was already started.</returns>
    bool StartWorkOnEntity(Entity entity);

    /// <summary>
    /// Gets the first entity for a specific assistance service type and location where work has not started yet.
    /// </summary>
    /// <param name="assistanceServiceType">The type of assistance service.</param>
    /// <param name="serviceLocation">The location of the assistance service instance.</param>
    /// <param name="maxRadius">The maximum radius to search for entities (optional, defaults to 200).</param>
    /// <returns>The first available entity, or null if none available.</returns>
    Entity GetFirstEntityOnWhichWorkHasNotStartedYet(Type assistanceServiceType, Vector3 serviceLocation, float maxRadius = 200f);

    /// <summary>
    /// Removes an entity from the sync service (e.g., when work is completed or entity is no longer valid).
    /// </summary>
    /// <param name="entityNetworkId">The network id of the entity to remove.</param>
    /// <returns>True if the entity was removed, false if it wasn't found.</returns>
    bool RemoveEntity(int entityNetworkId);

    /// <summary>
    /// Gets all entities for a specific assistance service type within a radius of a location.
    /// </summary>
    /// <param name="assistanceServiceType">The type of assistance service.</param>
    /// <param name="serviceLocation">The location of the assistance service instance.</param>
    /// <param name="maxRadius">The maximum radius to search for entities (optional, defaults to 200).</param>
    /// <returns>A list of entities with their work status.</returns>
    List<(Entity Entity, bool WorkStarted)> GetEntitiesInRadius(Type assistanceServiceType, Vector3 serviceLocation, float maxRadius = 200f);

    /// <summary>
    /// Checks if an entity is already being worked on by any assistance service.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <returns>True if work has started on this entity, false otherwise.</returns>
    bool IsEntityBeingWorkedOn(Entity entity);

    /// <summary>
    /// Cleans up invalid entities (entities that no longer exist in the game world).
    /// </summary>
    void CleanupInvalidEntities();
}
