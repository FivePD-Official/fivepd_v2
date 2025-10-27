using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using Serilog;
using Cfx = CitizenFX.Core.Native;
using Entity = CitizenFX.Core.Entity;

namespace FivePD.Server.Services;

// TODO: remove logs after testing

/// <inheritdoc cref="FivePD.Server.Interfaces.IGeneralAssistanceServiceSyncService" />
[Obfuscate]
public class GeneralAssistanceServiceSyncService : IGeneralAssistanceServiceSyncService, IOnDestroy
{
    private readonly ILogger _logger;
    private readonly ITickQueueService _tickQueueService;

    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(2);

    // Dictionary structure: AssistanceServiceType -> List of SyncedEntity
    private readonly ConcurrentDictionary<Type, List<SyncedEntity>> _syncedEntities = new();
    private readonly ConcurrentDictionary<int, SyncedEntity> _entityLookup = new();

    private DateTime _lastCleanup = DateTime.UtcNow;
    private Guid _cleanupTaskId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralAssistanceServiceSyncService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="tickQueueService">The <see cref="ITickQueueService"/> to use for scheduling periodic tasks.</param>
    public GeneralAssistanceServiceSyncService(ILogger logger, ITickQueueService tickQueueService)
    {
        this._logger = logger;
        this._tickQueueService = tickQueueService;
        this._cleanupTaskId = tickQueueService.EnqueueInterval(this.PerformPeriodicCleanup, (int)this._cleanupInterval.TotalMilliseconds);
    }

    /// <summary>
    /// Dequeues the cleanup task when the service is destroyed.
    /// </summary>
    void IOnDestroy.OnDestroy()
    {
        if (this._cleanupTaskId != Guid.Empty)
        {
            this._tickQueueService.DequeueInterval(this._cleanupTaskId);
            this._cleanupTaskId = Guid.Empty;
        }
    }

    /// <inheritdoc />
    public void AddEntity(Type assistanceServiceType, Entity entity, Vector3 serviceLocation, float maxRadius = 200f)
    {
        if (assistanceServiceType is null)
        {
            this._logger.Warning("Cannot add entity: assistanceServiceType is null");
            return;
        }

        // TODO DoesEntityExist does not work?? if (entity is null || !Cfx.API.DoesEntityExist(entity.NetworkId))
        if (entity is null)
        {
            this._logger.Warning("Cannot add entity: entity is null or does not exist");
            return;
        }

        // Check if entity is already tracked
        if (this._entityLookup.ContainsKey(entity.NetworkId))
        {
            this._logger.Debug("Entity {EntityNetworkId} is already being tracked", entity.NetworkId);
            return;
        }

        var syncedEntity = new SyncedEntity(entity, serviceLocation, maxRadius, assistanceServiceType);

        if (!this._syncedEntities.ContainsKey(assistanceServiceType))
        {
            this._syncedEntities[assistanceServiceType] = new List<SyncedEntity>();
        }

        this._syncedEntities[assistanceServiceType].Add(syncedEntity);

        this._entityLookup[entity.NetworkId] = syncedEntity;

        this._logger.Debug(
            "Added entity {EntityNetworkId} for service type {ServiceType} at location {Location} with radius {Radius}",
            entity.NetworkId,
            assistanceServiceType.Name,
            serviceLocation,
            maxRadius);
    }

    /// <inheritdoc />
    public void AddEntities(Type assistanceServiceType, IEnumerable<Entity> entities, Vector3 serviceLocation, float maxRadius = 200f)
    {
        if (assistanceServiceType is null || entities is null)
        {
            return;
        }

        foreach (var entity in entities)
        {
            this.AddEntity(assistanceServiceType, entity, serviceLocation, maxRadius);
        }
    }

    /// <inheritdoc />
    public bool StartWorkOnEntity(Entity entity)
    {
        if (entity is null)
        {
            this._logger.Warning("Cannot start work on entity: entity is null or does not exist");
            return false;
        }

        if (this._entityLookup.TryGetValue(entity.NetworkId, out var syncedEntity))
        {
            if (syncedEntity.WorkStarted)
            {
                this._logger.Debug("Work has already started on entity {EntityNetworkId}", entity.NetworkId);
                return false;
            }

            syncedEntity.WorkStarted = true;
            syncedEntity.WorkStartedAt = DateTime.UtcNow;
            this._logger.Debug("Started work on entity {EntityNetworkId}", entity.NetworkId);
            return true;
        }

        this._logger.Warning("Cannot start work on entity {EntityNetworkId}: entity not found in sync service", entity.NetworkId);
        return false;
    }

    /// <inheritdoc />
    public Entity GetFirstEntityOnWhichWorkHasNotStartedYet(Type assistanceServiceType, Vector3 serviceLocation, float maxRadius = 200f)
    {
        if (assistanceServiceType is null)
        {
            this._logger.Warning("Cannot get entity: assistanceServiceType is null");
            return null;
        }

        if (!this._syncedEntities.TryGetValue(assistanceServiceType, out var entities))
        {
            return null;
        }

        var availableEntity = entities
            .Where(e => !e.WorkStarted)
            .Where(e => this.IsWithinRadius(serviceLocation, e.ServiceLocation, Math.Min(maxRadius, e.MaxRadius)))
            .OrderBy(e => Vector3.Distance(serviceLocation, e.Entity.Position))
            .FirstOrDefault();

        return availableEntity?.Entity;
    }

    /// <inheritdoc />
    public bool RemoveEntity(int entityNetworkId)
    {
        if (!this._entityLookup.TryGetValue(entityNetworkId, out var syncedEntity))
        {
            this._logger.Debug("Entity {EntityNetworkId} not found in sync service", entityNetworkId);
            return false;
        }

        if (this._syncedEntities.TryGetValue(syncedEntity.AssistanceServiceType, out var entities))
        {
            entities.Remove(syncedEntity);

            if (entities.Count == 0)
            {
                this._syncedEntities.TryRemove(syncedEntity.AssistanceServiceType, out _);
            }
        }

        this._entityLookup.TryRemove(entityNetworkId, out _);

        this._logger.Debug("Removed entity {EntityNetworkId} from sync service", entityNetworkId);
        return true;
    }

    /// <inheritdoc />
    public List<(Entity Entity, bool WorkStarted)> GetEntitiesInRadius(Type assistanceServiceType, Vector3 serviceLocation, float maxRadius = 200f)
    {
        if (assistanceServiceType is null)
        {
            this._logger.Warning("Cannot get entities: assistanceServiceType is null");
            return new List<(Entity, bool)>();
        }

        if (!this._syncedEntities.TryGetValue(assistanceServiceType, out var entities))
        {
            return new List<(Entity, bool)>();
        }

        return entities
            .Where(e => this.IsWithinRadius(serviceLocation, e.ServiceLocation, Math.Min(maxRadius, e.MaxRadius)))
            .Select(e => (e.Entity, e.WorkStarted))
            .ToList();
    }

    /// <inheritdoc />
    public bool IsEntityBeingWorkedOn(Entity entity)
    {
        if (entity is null)
        {
            return false;
        }

        return this._entityLookup.TryGetValue(entity.NetworkId, out var syncedEntity) && syncedEntity.WorkStarted;
    }

    /// <inheritdoc />
    public void CleanupInvalidEntities()
    {
        var toRemove = new List<int>();

        foreach (var kvp in this._entityLookup)
        {
            var syncedEntity = kvp.Value;

            if (syncedEntity.WorkStarted && DateTime.UtcNow - syncedEntity.WorkStartedAt > TimeSpan.FromMinutes(10))
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var networkId in toRemove)
        {
            if (this._entityLookup.TryGetValue(networkId, out var syncedEntity) &&
                this._syncedEntities.TryGetValue(syncedEntity.AssistanceServiceType, out var entities))
            {
                entities.Remove(syncedEntity);

                if (entities.Count == 0)
                {
                    this._syncedEntities.TryRemove(syncedEntity.AssistanceServiceType, out _);
                }
            }

            this._entityLookup.TryRemove(networkId, out _);
        }

        if (toRemove.Count > 0)
        {
            this._logger.Debug("Cleaned up {Count} invalid entities from sync service", toRemove.Count);
        }
    }

    /// <summary>
    /// Checks if a location is within the specified radius of another location.
    /// </summary>
    /// <param name="location1">The first location.</param>
    /// <param name="location2">The second location.</param>
    /// <param name="maxRadius">The maximum radius.</param>
    /// <returns>True if within radius, false otherwise.</returns>
    private bool IsWithinRadius(Vector3 location1, Vector3 location2, float maxRadius)
    {
        return Vector3.Distance(location1, location2) <= maxRadius;
    }

    /// <summary>
    /// Performs the periodic cleanup of invalid entities.
    /// </summary>
    private void PerformPeriodicCleanup()
    {
        if (DateTime.UtcNow - this._lastCleanup > this._cleanupInterval)
        {
            this.CleanupInvalidEntities();
            this._lastCleanup = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Represents a synced entity with its metadata.
    /// </summary>
    private sealed record SyncedEntity
    {
        public SyncedEntity(Entity entity, Vector3 serviceLocation, float maxRadius, Type assistanceServiceType)
        {
            this.Entity = entity;
            this.ServiceLocation = serviceLocation;
            this.MaxRadius = maxRadius;
            this.AssistanceServiceType = assistanceServiceType;
            this.AddedAt = DateTime.UtcNow;
            this.WorkStarted = false;
        }

        public Entity Entity { get; }

        public Vector3 ServiceLocation { get; }

        public float MaxRadius { get; }

        public Type AssistanceServiceType { get; }

        public DateTime AddedAt { get; }

        public bool WorkStarted { get; set; }

        public DateTime WorkStartedAt { get; set; }
    }
}