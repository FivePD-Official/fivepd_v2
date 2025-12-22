using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Interfaces.VariableData;
using FivePD.Server.Models.Requests;
using FivePD.Server.Models.VariableData;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Debug = System.Diagnostics.Debug;

namespace FivePD.Server.Services.VariableData;

/// <inheritdoc cref="IVariableDataService" />
[Obfuscate]
public class VariableDataService : IVariableDataService, IInternalVariableDataService
{
    private readonly IDatabaseContextFactory _dbContextFactory;
    private readonly IPlayerHandlerService _playerHandlerService;
    private readonly INetworkEventService _networkEventService;

    private readonly List<VariableDataEntityObservable> _observables = new();
    private readonly Dictionary<Type, string> _activeSetTypes = new();
    private readonly List<object> _addonRepositoryInstances = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableDataService"/> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="IDatabaseContextFactory"/> to use.</param>
    /// <param name="playerHandlerService">The <see cref="IPlayerHandlerService"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public VariableDataService(
        IDatabaseContextFactory dbContextFactory,
        IPlayerHandlerService playerHandlerService,
        INetworkEventService networkEventService)
    {
        this._dbContextFactory = dbContextFactory;
        this._playerHandlerService = playerHandlerService;
        this._networkEventService = networkEventService;
    }

    /// <inheritdoc/>
    void IInternalVariableDataService.DisposeAddonRepositories()
    {
        foreach (var addonRepositoryInstance in this._addonRepositoryInstances)
        {
            ((IOnDestroy)addonRepositoryInstance).OnDestroy();
        }
    }

    /// <inheritdoc />
    public void RegisterAddonRepository<TVariableDataSet, TVariableDataRepository>(TVariableDataRepository repository)
        where TVariableDataSet : VariableDataSet
        where TVariableDataRepository : VariableDataRepository<TVariableDataSet>
    {
        this._addonRepositoryInstances.Add(repository);
        ((IOnInit)repository).OnInit();
    }

    /// <inheritdoc />
    public IDisposable OnAdded<TS>(Action<VariableDataEvent> observer)
    {
        return this.OnEvent<TS>(VariableDataEntityObservableType.Add, observer);
    }

    /// <inheritdoc />
    public IDisposable OnUpdated<TS>(Action<VariableDataEvent> observer)
    {
        return this.OnEvent<TS>(VariableDataEntityObservableType.Update, observer);
    }

    /// <inheritdoc />
    public IDisposable OnDeleted<TS>(Action<VariableDataEvent> observer)
    {
        return this.OnEvent<TS>(VariableDataEntityObservableType.Delete, observer);
    }

    /// <inheritdoc />
    public void RegisterSetAttributes<TS>(List<VariableDataAttribute> attributes)
        where TS : VariableDataSet
    {
        var assemblyName = this.GetExecutingAssemblyName();
        if (string.IsNullOrEmpty(assemblyName))
        {
            return;
        }

        if (assemblyName == "CitizenFX.Core")
        {
            assemblyName = "FivePD";
        }

        var setType = typeof(TS);
        this._activeSetTypes[setType] = assemblyName;

        var setIdAttr = setType.GetCustomAttribute<VariableDataSetIdAttribute>();
        if (setIdAttr is null)
        {
            return;
        }

        if (assemblyName != "FivePD" && setIdAttr.Id.StartsWith("fivepd_"))
        {
            return;
        }

        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var set = databaseContext.VariableDataSets
            .Include(set => set.Attributes)
            .Include(set => set.Entities)
            .ThenInclude(entities => entities.Values)
            .FirstOrDefault(set => set.Id == setIdAttr.Id);
        if (set is null)
        {
            set = Activator.CreateInstance<TS>();
            set.Id = setIdAttr.Id;
            databaseContext.VariableDataSets.Add(set);
        }

        foreach (var attr in attributes)
        {
            attr.AddonId = this._activeSetTypes[setType];
        }

        if (set.Attributes != null && set.Attributes.Any())
        {
            foreach (var attr in attributes.Where(attr => set.Attributes.All(setAttr => setAttr.Name != attr.Name)))
            {
                attr.Set = set;
                var addedAttributeEntry = databaseContext.VariableDataAttributes.Add(attr);
                databaseContext.SaveChanges();
                foreach (var entity in set.Entities)
                {
                    var value = new VariableDataValue
                    {
                        Entity = entity,
                        Attribute = addedAttributeEntry.Entity,
                        Value = JsonConvert.SerializeObject(
                            attr.DataType == VariableDataType.Date
                                ? DateTime.Now
                                : null),
                    };
                    databaseContext.VariableDataValues.Add(value);
                }
            }
        }
        else
        {
            set.Attributes = attributes;
        }

        databaseContext.SaveChanges();
    }

    /// <inheritdoc />
    public void UpdateSetAttribute<TS>(string attributeName, VariableDataAttribute newAttributeValues)
        where TS : VariableDataSet
    {
        var assemblyName = this.GetExecutingAssemblyName();
        if (string.IsNullOrEmpty(assemblyName))
        {
            return;
        }

        var setType = typeof(TS);
        if (!this._activeSetTypes.ContainsKey(setType))
        {
            return;
        }

        var setIdAttr = setType.GetCustomAttribute<VariableDataSetIdAttribute>();
        if (setIdAttr is null)
        {
            return;
        }

        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var attributeToUpdate = databaseContext.VariableDataAttributes
            .FirstOrDefault(attr => attr.Name == attributeName);
        if (attributeToUpdate is null)
        {
            return;
        }

        if (attributeToUpdate.AddonId == "FivePD Gamemode" || attributeToUpdate.AddonId == "FivePD" || attributeToUpdate.AddonId != assemblyName)
        {
            throw new ArgumentException("Can't update attribute of a different addon");
        }

        attributeToUpdate.Name = newAttributeValues.Name;
        attributeToUpdate.Label = newAttributeValues.Label;
        attributeToUpdate.HelpLabel = newAttributeValues.HelpLabel;
        attributeToUpdate.Placeholder = newAttributeValues.Placeholder;
        attributeToUpdate.Options = newAttributeValues.Options;
        attributeToUpdate.Readonly = newAttributeValues.Readonly;
        attributeToUpdate.Required = newAttributeValues.Required;
        attributeToUpdate.Order = newAttributeValues.Order;
        attributeToUpdate.ShowInGrid = newAttributeValues.ShowInGrid;
        databaseContext.SaveChanges();
    }

    /// <inheritdoc />
    public void DeleteSetAttribute<TS>(string attributeName)
        where TS : VariableDataSet
    {
        var assemblyName = this.GetExecutingAssemblyName();
        if (string.IsNullOrEmpty(assemblyName))
        {
            return;
        }

        var setType = typeof(TS);
        if (!this._activeSetTypes.ContainsKey(setType))
        {
            return;
        }

        var setIdAttr = setType.GetCustomAttribute<VariableDataSetIdAttribute>();
        if (setIdAttr is null)
        {
            return;
        }

        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var attributeToDelete = databaseContext.VariableDataAttributes
            .FirstOrDefault(attr => attr.Name == attributeName);
        if (attributeToDelete is null)
        {
            return;
        }

        if (attributeToDelete.AddonId == "FivePD Gamemode" || attributeToDelete.AddonId == "FivePD" || attributeToDelete.AddonId != assemblyName)
        {
            throw new ArgumentException("Can't delete attribute of a different addon");
        }

        var valuesToDelete = databaseContext.VariableDataValues
            .Include(value => value.Attribute)
            .Where(value => value.Attribute.Id == attributeToDelete.Id);
        databaseContext.VariableDataValues.RemoveRange(valuesToDelete);
        databaseContext.VariableDataAttributes.Remove(attributeToDelete);
        databaseContext.SaveChanges();
    }

    /// <inheritdoc />
    public VariableDataEntity AddEntity<TS>(VariableDataEntity entityToAdd)
        where TS : VariableDataSet
    {
        var setType = typeof(TS);
        var setIdAttr = setType.GetCustomAttribute<VariableDataSetIdAttribute>();
        if (setIdAttr is null)
        {
            return null;
        }

        return this.InternalAddEntity(null, setIdAttr.Id, entityToAdd, entityToAdd.MappedValues);
    }

    /// <inheritdoc />
    public void UpdateEntity(VariableDataEntity entityToUpdate)
    {
        var originalEntity = this.GetEntity(entityToUpdate.Id);
        if (originalEntity is null)
        {
            throw new ArgumentException("No entity exist with the given id");
        }

        this.InternalUpdateEntity(null, originalEntity.SetId, originalEntity.Id, entityToUpdate.MappedValues);
    }

    /// <inheritdoc />
    public VariableDataEntity GetEntity(string id)
    {
        return this.InternalGetEntity(id);
    }

    /// <inheritdoc />
    public VariableDataEntity GetEntity(string id, bool withTracking, bool queryCoreAttributeValue)
    {
        return this.InternalGetEntity(id, withTracking, queryCoreAttributeValue);
    }

    /// <inheritdoc />
    public List<VariableDataEntity> GetEntities<TS>(bool withTracking, bool queryCoreAttributeValue, Dictionary<string, object> filter = null)
        where TS : VariableDataSet
    {
        return this.GetEntitiesWithOptionalType<TS>(null, withTracking, queryCoreAttributeValue, filter);
    }

    /// <inheritdoc />
    public List<VariableDataEntity> GetEntities(Type setType, bool withTracking, bool queryCoreAttributeValue, Dictionary<string, object> filter = null)
    {
        return this.GetEntitiesWithOptionalType<VariableDataSet>(setType, withTracking, queryCoreAttributeValue, filter);
    }

    /// <inheritdoc />
    public void DeleteEntity(VariableDataEntity entityToDelete)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var entity = databaseContext.VariableDataEntities
            .Include(entity => entity.Values)
            .Include(entity => entity.Set)
            .FirstOrDefault(entity => entity.Id == entityToDelete.Id);
        if (entity is null)
        {
            return;
        }

        databaseContext.VariableDataEntities.Remove(entity);
        databaseContext.SaveChanges();
        this.InvokeEvent(
            VariableDataEntityObservableType.Delete,
            this.FindSetTypeById(entity.Set.Id),
            new VariableDataEvent
            {
                Entity = this.MapEntity(entity),
                Player = null,
            });
    }

    private VariableDataEntity InternalGetEntity(string id, bool withTracking = false, bool queryCoreAttributeValue = false)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var query = withTracking ? databaseContext.VariableDataEntities : databaseContext.VariableDataEntities.AsNoTracking();
        var entity = query
            .Include(entity => entity.Values)
            .ThenInclude(values => values.Attribute)
            .Include(entity => entity.Set)
            .ThenInclude(set => set.Attributes)
            .FirstOrDefault(entity => entity.Id == id);

        var mappedEntity = entity is null ? null : this.MapEntity(entity, queryCoreAttributeValue);
        return mappedEntity;
    }

    private List<VariableDataEntity> GetEntitiesWithOptionalType<TS>(
        Type setType,
        bool withTracking,
        bool queryCoreAttributeValues,
        IReadOnlyDictionary<string, object> filter = null)
        where TS : VariableDataSet
    {
        var setIdAttr = (setType ?? typeof(TS)).GetCustomAttribute<VariableDataSetIdAttribute>();
        if (setIdAttr is null)
        {
            return new List<VariableDataEntity>();
        }

        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var query = withTracking ? databaseContext.VariableDataEntities : databaseContext.VariableDataEntities.AsNoTracking();
        var entities = query
            .Include(entity => entity.Values)
            .ThenInclude(v => v.Attribute)
            .Include(entity => entity.Set)
            .ThenInclude(set => set.Attributes)
            .Where(entity => entity.Set.Id == setIdAttr.Id)
            .ToList();

        if (filter is null)
        {
            return entities.Select(x => this.MapEntity(x, queryCoreAttributeValues)).ToList();
        }

        var itemsToRemove = new List<VariableDataEntity>();
        foreach (var entity in entities)
        {
            foreach (var value in entity.Values)
            {
                if (!filter.TryGetValue(value.Attribute.Name, out var filterValue))
                {
                    continue;
                }

                var attributeValue = JsonConvert.DeserializeObject(value.Value);
                if (attributeValue?.GetType() == typeof(JArray) && filterValue.GetType() == typeof(JArray))
                {
                    var attributeValueArray = (JArray)attributeValue;
                    var filterValueArray = (JArray)filterValue;
                    if (!attributeValueArray.Any(x => filterValueArray.Any(y => y.ToString() == x.ToString())))
                    {
                        itemsToRemove.Add(entity);
                    }

                    continue;
                }

                if (attributeValue is IConvertible testValueConvertible && filterValue is IConvertible)
                {
                    attributeValue = testValueConvertible.ToType(
                        filterValue.GetType(),
                        CultureInfo.InvariantCulture);
                    if (!Equals(attributeValue, filterValue))
                    {
                        itemsToRemove.Add(entity);
                    }
                }
                else if (attributeValue != filterValue)
                {
                    itemsToRemove.Add(entity);
                }
            }
        }

        foreach (var itemToRemove in itemsToRemove)
        {
            entities.Remove(itemToRemove);
        }

        return entities.Select(x => this.MapEntity(x, queryCoreAttributeValues)).ToList();
    }

    private Page<VariableDataEntity> GetEntitiesPage(Type setType, int pageNumber, Filters<VariableDataEntity> filters)
    {
        var setIdAttr = setType.GetCustomAttribute<VariableDataSetIdAttribute>();
        if (setIdAttr is null)
        {
            return new Page<VariableDataEntity>();
        }

        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var page = databaseContext.VariableDataEntities
            .Include(entity => entity.Values)
            .Include(entity => entity.Set)
            .ThenInclude(set => set.Attributes)
            .Where(entity => entity.Set.Id == setIdAttr.Id)
            .Paginate(pageNumber, filters);

        page.Items = page.Items.Select(x => this.MapEntity(x)).ToList();
        return page;
    }

    private VariableDataEntity MapEntity(VariableDataEntity entity, bool queryCoreAttributeValues = true)
    {
        if (entity.Set?.Attributes is null || entity.Values is null)
        {
            return null;
        }

        entity.MappedValues = new Dictionary<string, object>();
        foreach (var value in entity.Values.Where(a =>
                     a.Attribute.AddonId == VariableDataAttribute.AddedByAdminAddonId || this._activeSetTypes.ContainsValue(a.Attribute.AddonId)))
        {
            var deserializedValue = JsonConvert.DeserializeObject(value.Value);
            entity.MappedValues[value.Attribute.Name] = deserializedValue;
        }

        if (!queryCoreAttributeValues)
        {
            return null;
        }

        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        if (entity.MappedValues.ContainsKey("DepartmentId"))
        {
            var value = databaseContext.Departments.AsNoTracking()
                .FirstOrDefault(dept => dept.Id == (entity.MappedValues["DepartmentId"] as long? ?? 0));
            entity.MappedValues["Department"] = value;
        }

        if (entity.MappedValues.ContainsKey("DepartmentIds"))
        {
            var ids = (entity.MappedValues["DepartmentIds"] as JArray)?.Select(x => long.Parse(x.ToString())).ToList();
            var value = databaseContext.Departments.AsNoTracking().Where(department => ids.Contains(department.Id)).ToList();
            entity.MappedValues["DepartmentIds"] = value;
        }

        if (entity.MappedValues.ContainsKey("UserId"))
        {
            var value = databaseContext.Users.AsNoTracking().FirstOrDefault(user => user.Id == (entity.MappedValues["UserId"] as long? ?? 0));
            entity.MappedValues["User"] = value;
        }

        if (entity.MappedValues.ContainsKey("UserIds"))
        {
            var ids = (entity.MappedValues["UserIds"] as JArray)?.Select(x => long.Parse(x.ToString())).ToList();
            var value = databaseContext.Users.AsNoTracking().Where(user => ids.Contains(user.Id)).ToList();
            entity.MappedValues["Users"] = value;
        }

        if (entity.MappedValues.ContainsKey("IncidentHistoryId"))
        {
            var value = databaseContext.IncidentHistories.AsNoTracking()
                .FirstOrDefault(user => user.Id == entity.MappedValues["IncidentHistoryId"] as string);
            entity.MappedValues["IncidentHistory"] = value;
        }

        if (entity.MappedValues.ContainsKey("IncidentHistoryIds"))
        {
            var ids = (entity.MappedValues["IncidentHistoryIds"] as JArray)?.Select(x => x.ToString()).ToList();
            var value = databaseContext.IncidentHistories.AsNoTracking().Where(incidentHistory => ids.Contains(incidentHistory.Id)).ToList();
            entity.MappedValues["IncidentHistories"] = value;
        }

        if (entity.MappedValues.ContainsKey("PedHistoryId"))
        {
            var value = databaseContext.PedData.AsNoTracking()
                .FirstOrDefault(pedHistory => pedHistory.Id == (entity.MappedValues["PedHistoryId"] as long? ?? 0));
            entity.MappedValues["PedHistory"] = value;
        }

        if (entity.MappedValues.ContainsKey("PedHistoryIds"))
        {
            var ids = (entity.MappedValues["PedHistoryIds"] as JArray)?.Select(x => long.Parse(x.ToString())).ToList();
            var value = databaseContext.PedData.AsNoTracking().Where(pedHistory => ids.Contains(pedHistory.Id)).ToList();
            entity.MappedValues["PedHistories"] = value;
        }

        return entity;
    }

    [UIEvent(Events.UI.VariableData.GetSetAttribute)]
    private void GetSetAttributeFromNui(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var attribute = databaseContext.VariableDataAttributes
            .AsNoTracking()
            .FirstOrDefault(set => set.Id == request.Id);

        if (attribute is null)
        {
            this._networkEventService.TriggerEvent(player, guid, null);
            return;
        }

        attribute.Set = null;
        this._networkEventService.TriggerEventSerialized(player, guid, attribute);
    }

    [UIEvent(Events.UI.VariableData.GetSetAttributes)]
    private void GetSetAttributesFromNui(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<GetSetRequest>(serializedRequest);
        var set = databaseContext.VariableDataSets
            .AsNoTracking()
            .Include(set => set.Attributes)
            .FirstOrDefault(set => set.Id == request.SetId);

        if (set is null)
        {
            this._networkEventService.TriggerEvent(player, guid, null);
            return;
        }

        foreach (var attribute in set.Attributes)
        {
            attribute.Set = null;
        }

        this._networkEventService.TriggerEventSerialized(player, guid, set.Attributes);
    }

    [UIEvent(Events.UI.VariableData.GetSetAttributesPageable)]
    private void GetSetAttributesPageableFromNui(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<GetSetRequest>(serializedRequest);
        var page = databaseContext.VariableDataAttributes
            .AsNoTracking()
            .Include(attr => attr.Set)
            .Where(attr => attr.Set.Id == request.SetId)
            .Paginate(request.Page, new Filters<VariableDataAttribute>());

        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.VariableData.AddSetAttribute)]
    private void AddSetAttributeFromNui(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<AddSetAttributeRequest>(serializedRequest);
        var set = databaseContext.VariableDataSets
            .Include(set => set.Attributes)
            .Include(set => set.Entities)
            .ThenInclude(entities => entities.Values)
            .FirstOrDefault(set => set.Id == request.SetId);
        if (set is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        if (!string.IsNullOrEmpty(request.AttributeType))
        {
            VariableDataAttribute coreAttribute = request.AttributeType switch
            {
                "UserId" => new CoreAttributes.UserId(),
                "UserIds" => new CoreAttributes.UserIds(),
                "DepartmentId" => new CoreAttributes.DepartmentId(),
                "DepartmentIds" => new CoreAttributes.DepartmentIds(),
                "IncidentHistoryId" => new CoreAttributes.IncidentHistoryId(),
                "IncidentHistoryIds" => new CoreAttributes.IncidentHistoryIds(),
                "PedHistoryId" => new CoreAttributes.PedHistoryId(),
                "PedHistoryIds" => new CoreAttributes.PedHistoryIds(),
                _ => null,
            };
            if (coreAttribute is not null)
            {
                request.Attribute.Name = coreAttribute.Name;
                request.Attribute.DataType = coreAttribute.DataType;
            }
        }

        if (set.Attributes.Any(attr => attr.Name == request.Attribute.Name))
        {
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "ERROR.ATTRIBUTE.CREATE.TITLE", "ERROR.ATTRIBUTE.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        request.Attribute.AddonId = VariableDataAttribute.AddedByAdminAddonId;
        request.Attribute.Set = set;

        var addedAttributeEntry = databaseContext.VariableDataAttributes.Add(request.Attribute);
        databaseContext.SaveChanges();

        foreach (var entity in set.Entities)
        {
            var value = new VariableDataValue
            {
                Entity = entity,
                Attribute = addedAttributeEntry.Entity,
                Value = JsonConvert.SerializeObject(
                    request.Attribute.DataType == VariableDataType.Date
                        ? DateTime.Now
                        : null),
            };
            databaseContext.VariableDataValues.Add(value);
        }

        databaseContext.SaveChanges();
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.CREATE",
            "LABEL.ATTRIBUTE.SUCCESS.CREATE",
            new Dictionary<string, string>
            {
                { "attribute", request.Attribute.Name },
            });
    }

    [UIEvent(Events.UI.VariableData.EditSetAttribute)]
    private void EditSetAttributeFromNui(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<EditSetAttributeRequest>(serializedRequest);

        var set = databaseContext.VariableDataSets
            .Include(set => set.Attributes)
            .FirstOrDefault(set => set.Id == request.SetId);
        if (set is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var attribute = databaseContext.VariableDataAttributes.FirstOrDefault(attr => attr.Id == request.Attribute.Id);
        if (attribute is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        if (set.Attributes.Where(attr => attr.Id != attribute.Id).Any(attr => attr.Name == request.Attribute.Name))
        {
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "ERROR.ATTRIBUTE.EDIT.TITLE", "ERROR.ATTRIBUTE.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var attributeOldName = attribute.Name;
        if (!string.IsNullOrEmpty(request.Attribute.Name))
        {
            attribute.Name = request.Attribute.Name;
        }

        attribute.Label = request.Attribute.Label;
        attribute.HelpLabel = request.Attribute.HelpLabel;
        attribute.Placeholder = request.Attribute.Placeholder;
        attribute.Required = request.Attribute.Required;
        attribute.Readonly = request.Attribute.Readonly;
        attribute.Hidden = request.Attribute.Hidden;
        attribute.ShowInGrid = request.Attribute.ShowInGrid;
        databaseContext.SaveChanges();
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.EDIT",
            "LABEL.ATTRIBUTE.SUCCESS.EDIT",
            new Dictionary<string, string>
            {
                { "attribute", attributeOldName },
            });
    }

    [UIEvent(Events.UI.VariableData.DeleteSetAttribute)]
    private void DeleteSetAttributeFromNui(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<DeleteSetAttributeRequest>(serializedRequest);
        var set = databaseContext.VariableDataSets
            .Include(set => set.Attributes)
            .Include(set => set.Entities)
            .ThenInclude(entities => entities.Values)
            .ThenInclude(v => v.Attribute)
            .FirstOrDefault(set => set.Id == request.SetId);
        if (set is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var attributeToDelete = databaseContext.VariableDataAttributes
            .FirstOrDefault(attr => attr.Id == request.AttributeId);
        if (attributeToDelete is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        foreach (var entity in set.Entities)
        {
            foreach (var value in entity.Values.Where(value => value.Attribute.Id == request.AttributeId))
            {
                databaseContext.VariableDataValues.Remove(value);
            }
        }

        var attributeName = attributeToDelete.Name;
        databaseContext.VariableDataAttributes.Remove(attributeToDelete);
        databaseContext.SaveChanges();
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.DELETE", "LABEL.ATTRIBUTE.SUCCESS.DELETE", new Dictionary<string, string>
        {
            { "attribute", attributeName },
        });
    }

    [UIEvent(Events.UI.VariableData.GetEntity)]
    private void GetEntityFromNui(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<GetSetEntityRequest>(serializedRequest);
        var setType = this.FindSetTypeById(request.SetId);
        if (setType is null)
        {
            this._networkEventService.TriggerEvent(player, guid, null);
            return;
        }

        var entity = this.InternalGetEntity(request.Id, queryCoreAttributeValue: true);
        this._networkEventService.TriggerEventSerialized(player, guid, entity);
    }

    [UIEvent(Events.UI.VariableData.GetEntitiesPageable)]
    private void GetEntitiesPageableUI(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<GetSetEntitiesPageableRequest>(serializedRequest);
        var setType = this.FindSetTypeById(request.SetId);
        if (setType is null)
        {
            this._networkEventService.TriggerEvent(player, guid, null);
            return;
        }

        var filters = new Filters<VariableDataEntity>();
        var entities = this.GetEntitiesPage(setType, request.Page, filters);
        this._networkEventService.TriggerEventSerialized(player, guid, entities);
    }

    [UIEvent(Events.UI.VariableData.GetEntities)]
    private void GetEntitiesUI(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<GetSetEntitiesRequest>(serializedRequest);
        var setType = this.FindSetTypeById(request.SetId);
        if (setType is null)
        {
            this._networkEventService.TriggerEvent(player, guid, null);
            return;
        }

        var entities = this.GetEntities(setType, false, true, request.Filter);
        foreach (var entity in entities)
        {
            entity.Values = null;
        }

        this._networkEventService.TriggerEventSerialized(player, guid, entities);
    }

    [UIEvent(Events.UI.VariableData.AddEntity)]
    private void AddEntityFromNui(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AddEntityRequest>(serializedRequest);
        var setType = this.FindSetTypeById(request.SetId);
        if (setType is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var addedEntity = this.InternalAddEntity(player, request.SetId, new VariableDataEntity(), request.Entity);
        if (addedEntity is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.CREATE", "LABEL.ENTITY.SUCCESS.CREATE");
    }

    [UIEvent(Events.UI.VariableData.EditEntity)]
    private void UpdateEntityFromNui(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AddEntityRequest>(serializedRequest);
        var setType = this.FindSetTypeById(request.SetId);
        if (setType is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this.InternalUpdateEntity(player, request.SetId, request.EntityId, request.Entity);
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.EDIT", "LABEL.ENTITY.SUCCESS.EDIT");
    }

    [UIEvent(Events.UI.VariableData.DeleteEntity)]
    private void DeleteEntityFromNui(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<DeleteEntityRequest>(serializedRequest);
        var setType = this.FindSetTypeById(request.SetId);
        if (setType is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var entityToDelete = databaseContext.VariableDataEntities
            .FirstOrDefault(entity => entity.Id == request.Id);
        if (entityToDelete is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        databaseContext.VariableDataEntities.Remove(entityToDelete);
        databaseContext.SaveChanges();
        this.InvokeEvent(
            VariableDataEntityObservableType.Delete,
            setType,
            new VariableDataEvent
            {
                Entity = entityToDelete,
                Player = FPlayer.FromPlayer(player),
            });
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.DELETE", "LABEL.ENTITY.SUCCESS.DELETE");
    }

    private VariableDataEntity InternalAddEntity(Player player, string setId, VariableDataEntity entityToAdd, Dictionary<string, object> valuesToAdd)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var set = databaseContext.VariableDataSets
            .Include(s => s.Attributes)
            .FirstOrDefault(s => s.Id == setId);
        if (set is null)
        {
            return null;
        }

        var attributeValueMap = new Dictionary<VariableDataAttribute, object>();
        foreach (var attr in set.Attributes)
        {
            object value = null;
            if (attr.DataType == VariableDataType.Date)
            {
                value = DateTime.Now;
            }

            attributeValueMap[attr] = value;
        }

        foreach (var value in valuesToAdd)
        {
            var attribute = set.Attributes.FirstOrDefault(a => a.Name == value.Key);
            if (attribute is null)
            {
                continue;
            }

            attributeValueMap[attribute] = value.Value;
        }

        if (player is not null)
        {
            var userId = attributeValueMap.Keys.FirstOrDefault(x => x.Name == "UserId");
            if (userId is not null)
            {
                var user = this._playerHandlerService.FindUserOfPlayer(player);
                attributeValueMap[userId] = user?.Id ?? 0;
            }
        }

        var values = attributeValueMap.Select(item => new VariableDataValue
            {
                Attribute = item.Key,
                Entity = entityToAdd,
                Value = JsonConvert.SerializeObject(item.Value),
            })
            .ToList();

        entityToAdd.Set = set;
        databaseContext.VariableDataValues.AddRange(values);
        var entityEntry = databaseContext.VariableDataEntities.Add(entityToAdd);
        databaseContext.SaveChanges();
        this.InvokeEvent(
            VariableDataEntityObservableType.Add,
            this.FindSetTypeById(setId),
            new VariableDataEvent
            {
                Entity = this.MapEntity(entityEntry.Entity),
                Player = FPlayer.FromPlayer(player),
            });
        return entityEntry.Entity;
    }

    private void InternalUpdateEntity(Player player, string setId, string entityId, Dictionary<string, object> newValues)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var set = databaseContext.VariableDataSets
            .Include(set => set.Attributes)
            .FirstOrDefault(set => set.Id == setId);
        if (set is null)
        {
            return;
        }

        var attributeValueMap = new Dictionary<string, object>();
        foreach (var attr in set.Attributes)
        {
            attributeValueMap[attr.Name] = null;
        }

        foreach (var value in newValues)
        {
            var attribute = set.Attributes.FirstOrDefault(a => a.Name == value.Key);
            if (attribute is null)
            {
                continue;
            }

            attributeValueMap[attribute.Name] = value.Value;
        }

        var entityValues = databaseContext.VariableDataValues
            .Include(value => value.Entity)
            .Include(value => value.Attribute)
            .Where(value => value.Entity.Id == entityId)
            .ToList();
        foreach (var dataValue in entityValues)
        {
            dataValue.Value = JsonConvert.SerializeObject(attributeValueMap[dataValue.Attribute.Name]);
        }

        databaseContext.SaveChanges();
        this.InvokeEvent(
            VariableDataEntityObservableType.Update,
            this.FindSetTypeById(setId),
            new VariableDataEvent
            {
                Entity = this.InternalGetEntity(entityId),
                Player = FPlayer.FromPlayer(player),
            });
    }

    private Type FindSetTypeById(string setId)
    {
        var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
        var type = assemblyTypes
            .Where(type => type.IsClass)
            .Where(type => type.GetCustomAttribute<VariableDataSetIdAttribute>() != null)
            .FirstOrDefault(type => type.GetCustomAttribute<VariableDataSetIdAttribute>().Id == setId);

        return type;
    }

    private IDisposable OnEvent<TS>(VariableDataEntityObservableType observableType, Action<VariableDataEvent> observer)
    {
        var type = typeof(TS);
        var observable = this._observables.FirstOrDefault(o => o.SetType == type && o.Type == observableType);
        if (observable is null)
        {
            observable = new VariableDataEntityObservable(type, observableType);
            this._observables.Add(observable);
        }

        return observable.Subscribe(@event => { observer?.Invoke(@event); });
    }

    private void InvokeEvent(VariableDataEntityObservableType observableType, Type setType, VariableDataEvent @event)
    {
        var observable = this._observables.FirstOrDefault(o => o.Type == observableType && o.SetType == setType);
        observable?.InvokeObservers(@event);
    }

    private string GetExecutingAssemblyName()
    {
        // Technically, the executing assembly is the server. Sort through the stack trace and find a ref. to the actual addon
        foreach (var frame in new StackTrace().GetFrames())
        {
            try
            {
                if (frame.GetMethod().DeclaringType!.Assembly != Assembly.GetExecutingAssembly())
                {
                    return frame.GetMethod().DeclaringType!.Assembly.GetName().Name;
                }
            }
            catch (Exception)
            {
                // Continues when frame assembly is null
            }
        }

        return null;
    }
}