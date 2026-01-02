using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using Dapper;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Interfaces.VariableData;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.Models.VariableData;
using FivePD.Server.QueryHelpers;
using FivePD.Server.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FivePD.Server.Services.VariableData;

/// <inheritdoc cref="IVariableDataService" />
[Obfuscate]
public class VariableDataService : IVariableDataService, IInternalVariableDataService
{
    private readonly VariableDataSetRepository _variableDataSetRepository;
    private readonly VariableDataEntityRepository _variableDataEntityRepository;
    private readonly VariableDataAttributeRepository _variableDataAttributeRepository;
    private readonly VariableDataValueRepository _variableDataValueRepository;
    private readonly IPlayerHandlerService _playerHandlerService;
    private readonly INetworkEventService _networkEventService;
    private readonly IDatabaseConnectionFactory _dbConnectionFactory;

    private readonly List<VariableDataEntityObservable> _observables = new();
    private readonly Dictionary<Type, string> _activeSetTypes = new();
    private readonly List<object> _addonRepositoryInstances = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableDataService"/> class.
    /// </summary>
    /// <param name="variableDataSetRepository">The <see cref="VariableDataSetRepository"/> to use.</param>
    /// <param name="variableDataEntityRepository">The <see cref="VariableDataEntityRepository"/> to use.</param>
    /// <param name="variableDataAttributeRepository">The <see cref="VariableDataAttributeRepository"/> to use.</param>
    /// <param name="variableDataValueRepository">The <see cref="VariableDataValueRepository"/> to use.</param>
    /// <param name="playerHandlerService">The <see cref="IPlayerHandlerService"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    /// <param name="dbConnectionFactory">The <see cref="IDatabaseConnectionFactory"/> to use.</param>
    public VariableDataService(
        VariableDataSetRepository variableDataSetRepository,
        VariableDataEntityRepository variableDataEntityRepository,
        VariableDataAttributeRepository variableDataAttributeRepository,
        VariableDataValueRepository variableDataValueRepository,
        IPlayerHandlerService playerHandlerService,
        INetworkEventService networkEventService,
        IDatabaseConnectionFactory dbConnectionFactory)
    {
        this._variableDataSetRepository = variableDataSetRepository;
        this._variableDataEntityRepository = variableDataEntityRepository;
        this._variableDataAttributeRepository = variableDataAttributeRepository;
        this._variableDataValueRepository = variableDataValueRepository;
        this._playerHandlerService = playerHandlerService;
        this._networkEventService = networkEventService;
        this._dbConnectionFactory = dbConnectionFactory;
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
        where TVariableDataRepository : VariableDataBaseService<TVariableDataSet>
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

        var set = this._variableDataSetRepository.GetSetWithAttributesAndEntities(setIdAttr.Id);
        if (set is null)
        {
            set = Activator.CreateInstance<TS>();
            set.Id = setIdAttr.Id;
            this._variableDataSetRepository.Add(set);
        }

        foreach (var attr in attributes)
        {
            attr.AddonId = this._activeSetTypes[setType];
        }

        if (set.Attributes != null && set.Attributes.Any())
        {
            foreach (var attr in attributes.Where(attr => set.Attributes.All(setAttr => setAttr.Name != attr.Name)))
            {
                attr.SetId = set.Id;
                this._variableDataAttributeRepository.Add(attr);
                foreach (var entity in set.Entities)
                {
                    var value = new VariableDataValue
                    {
                        EntityId = entity.Id,
                        AttributeId = attr.Id,
                        Value = JsonConvert.SerializeObject(
                            attr.DataType == VariableDataType.Date
                                ? DateTime.Now
                                : null),
                    };
                    this._variableDataValueRepository.Add(value);
                }
            }
        }
        else
        {
            foreach (var attr in attributes)
            {
                attr.SetId = set.Id;
                this._variableDataAttributeRepository.Add(attr);
            }
        }
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

        var attributeToUpdate = this._variableDataAttributeRepository.GetAttributeBySetIdAndName(setIdAttr.Id, attributeName);
        if (attributeToUpdate is null)
        {
            return;
        }

        if (attributeToUpdate.AddonId == "FivePD Gamemode" || attributeToUpdate.AddonId == "FivePD" || attributeToUpdate.AddonId != assemblyName)
        {
            throw new ArgumentException("Can't update attribute of a different addon");
        }

        this._variableDataAttributeRepository.Update(attributeToUpdate, x =>
        {
            x.Name = newAttributeValues.Name;
            x.Label = newAttributeValues.Label;
            x.HelpLabel = newAttributeValues.HelpLabel;
            x.Placeholder = newAttributeValues.Placeholder;
            x.Options = newAttributeValues.Options;
            x.Readonly = newAttributeValues.Readonly;
            x.Required = newAttributeValues.Required;
            x.DisplayOrder = newAttributeValues.DisplayOrder;
            x.ShowInGrid = newAttributeValues.ShowInGrid;
        });
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

        var attributeToDelete = this._variableDataAttributeRepository.GetAttributeBySetIdAndName(setIdAttr.Id, attributeName);
        if (attributeToDelete is null)
        {
            return;
        }

        if (attributeToDelete.AddonId == "FivePD Gamemode" || attributeToDelete.AddonId == "FivePD" || attributeToDelete.AddonId != assemblyName)
        {
            throw new ArgumentException("Can't delete attribute of a different addon");
        }

        this._variableDataAttributeRepository.Delete(attributeToDelete.Id);
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
    public VariableDataEntity GetEntity(string id, bool queryCoreAttributeValue)
    {
        return this.InternalGetEntity(id, queryCoreAttributeValue);
    }

    /// <inheritdoc />
    public List<VariableDataEntity> GetEntities<TS>(bool queryCoreAttributeValue, Dictionary<string, object> filter = null)
        where TS : VariableDataSet
    {
        return this.GetEntitiesWithOptionalType<TS>(null, queryCoreAttributeValue, filter);
    }

    /// <inheritdoc />
    public List<VariableDataEntity> GetEntities(Type setType, bool queryCoreAttributeValue, Dictionary<string, object> filter = null)
    {
        return this.GetEntitiesWithOptionalType<VariableDataSet>(setType, queryCoreAttributeValue, filter);
    }

    /// <inheritdoc />
    public void DeleteEntity(VariableDataEntity entityToDelete)
    {
        var entity = this._variableDataEntityRepository.GetEntityWithValues(entityToDelete.Id);
        if (entity is null)
        {
            return;
        }

        this._variableDataEntityRepository.Delete(entity.Id);
        this.InvokeEvent(
            VariableDataEntityObservableType.Delete,
            this.FindSetTypeById(entity.Set.Id),
            new VariableDataEvent
            {
                Entity = this.MapEntity(entity),
                Player = null,
            });
    }

    private VariableDataEntity InternalGetEntity(string id, bool queryCoreAttributeValue = false)
    {
        var entity = this._variableDataEntityRepository.GetEntityWithValues(id);
        var mappedEntity = entity is null ? null : this.MapEntity(entity, queryCoreAttributeValue);
        return mappedEntity;
    }

    private List<VariableDataEntity> GetEntitiesWithOptionalType<TS>(
        Type setType,
        bool queryCoreAttributeValues,
        IReadOnlyDictionary<string, object> filter = null)
        where TS : VariableDataSet
    {
        var setIdAttr = (setType ?? typeof(TS)).GetCustomAttribute<VariableDataSetIdAttribute>();
        if (setIdAttr is null)
        {
            return new List<VariableDataEntity>();
        }

        var entities = this._variableDataEntityRepository.GetEntitiesBySetId(setIdAttr.Id);
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

    private Page<VariableDataEntity> GetEntitiesPage(Type setType, int pageNumber)
    {
        var setIdAttr = setType.GetCustomAttribute<VariableDataSetIdAttribute>();
        if (setIdAttr is null)
        {
            return new Page<VariableDataEntity>();
        }

        var page = this._variableDataEntityRepository.Paginate(pageNumber);
        var newItems = new List<VariableDataEntity>();
        foreach (var entity in page.Items)
        {
            newItems.Add(this._variableDataEntityRepository.GetEntityWithValues(entity.Id));
        }

        page.Items = newItems.Select(x => this.MapEntity(x)).ToList();
        return page;
    }

    private VariableDataEntity MapEntity(VariableDataEntity entity, bool queryCoreAttributeValues = true)
    {
        if (entity.Values is null)
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

        using var connection = this._dbConnectionFactory.CreateScope().Connection;

        if (entity.MappedValues.ContainsKey("DepartmentId"))
        {
            var builderResult = SqlBuilder
                .From(TableNames.Departments)
                .WhereEquals("Id", Convert.ToInt32(entity.MappedValues["DepartmentId"]))
                .Build();
            entity.MappedValues["Department"] = connection.QueryFirstOrDefault<Department>(builderResult.Sql, builderResult.Parameters);
        }

        if (entity.MappedValues.ContainsKey("DepartmentIds"))
        {
            var ids = (entity.MappedValues["DepartmentIds"] as JArray)?.Select(Convert.ToInt32);
            var builderResult = SqlBuilder
                .From(TableNames.Departments)
                .WhereIn("Id", ids)
                .Build();
            entity.MappedValues["Departments"] = connection.Query<Department>(builderResult.Sql, builderResult.Parameters);
        }

        if (entity.MappedValues.ContainsKey("UserId"))
        {
            var builderResult = SqlBuilder
                .From(TableNames.Users)
                .WhereEquals("Id", Convert.ToInt32(entity.MappedValues["UserId"]))
                .Build();
            entity.MappedValues["User"] = connection.QueryFirstOrDefault<User>(builderResult.Sql, builderResult.Parameters);
        }

        if (entity.MappedValues.ContainsKey("UserIds"))
        {
            var ids = (entity.MappedValues["UserIds"] as JArray)?.Select(Convert.ToInt32);
            var builderResult = SqlBuilder
                .From(TableNames.Users)
                .WhereIn("Id", ids)
                .Build();
            entity.MappedValues["Users"] = connection.Query<User>(builderResult.Sql, builderResult.Parameters);
        }

        if (entity.MappedValues.ContainsKey("IncidentHistoryId"))
        {
            var builderResult = SqlBuilder
                .From(TableNames.IncidentHistories)
                .WhereEquals("Id", Convert.ToInt32(entity.MappedValues["IncidentHistoryId"]))
                .Build();
            entity.MappedValues["IncidentHistory"] = connection.QueryFirstOrDefault<IncidentHistory>(builderResult.Sql, builderResult.Parameters);
        }

        if (entity.MappedValues.ContainsKey("IncidentHistoryIds"))
        {
            var ids = (entity.MappedValues["IncidentHistoryIds"] as JArray)?.Select(Convert.ToInt32);
            var builderResult = SqlBuilder
                .From(TableNames.IncidentHistories)
                .WhereIn("Id", ids)
                .Build();
            entity.MappedValues["IncidentHistories"] = connection.Query<IncidentHistory>(builderResult.Sql, builderResult.Parameters);
        }

        if (entity.MappedValues.ContainsKey("PedHistoryId"))
        {
            var builderResult = SqlBuilder
                .From(TableNames.PedData)
                .WhereEquals("Id", Convert.ToInt32(entity.MappedValues["PedHistoryId"]))
                .Build();
            entity.MappedValues["PedData"] = connection.QueryFirstOrDefault<PedData>(builderResult.Sql, builderResult.Parameters);
        }

        if (entity.MappedValues.ContainsKey("PedHistoryIds"))
        {
            var ids = (entity.MappedValues["PedHistoryIds"] as JArray)?.Select(Convert.ToInt32);
            var builderResult = SqlBuilder
                .From(TableNames.PedData)
                .WhereIn("Id", ids)
                .Build();
            entity.MappedValues["PedHistories"] = connection.Query<PedData>(builderResult.Sql, builderResult.Parameters);
        }

        return entity;
    }

    [UIEvent(Events.UI.VariableData.GetSetAttribute)]
    private void GetSetAttributeFromNui(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var attribute = this._variableDataAttributeRepository.FindById(request.Id);
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
        var request = JsonConvert.DeserializeObject<GetSetRequest>(serializedRequest);
        var set = this._variableDataSetRepository.GetSetWithAttributesAndEntities(request.SetId);
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
        var request = JsonConvert.DeserializeObject<GetSetRequest>(serializedRequest);
        var page = this._variableDataAttributeRepository.Paginate(request.Page, b => b.WhereEquals("SetId", request.SetId));
        foreach (var item in page.Items)
        {
            item.Set = this._variableDataSetRepository.FindById(item.SetId);
        }

        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.VariableData.AddSetAttribute)]
    private void AddSetAttributeFromNui(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AddSetAttributeRequest>(serializedRequest);
        var set = this._variableDataSetRepository.GetSetWithAttributesAndEntities(request.SetId);
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
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                false,
                "ERROR.ATTRIBUTE.CREATE.TITLE",
                "ERROR.ATTRIBUTE.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        request.Attribute.AddonId = VariableDataAttribute.AddedByAdminAddonId;
        request.Attribute.SetId = set.Id;

        this._variableDataAttributeRepository.Add(request.Attribute);
        foreach (var entity in set.Entities)
        {
            var value = new VariableDataValue
            {
                EntityId = entity.Id,
                AttributeId = request.Attribute.Id,
                Value = JsonConvert.SerializeObject(
                    request.Attribute.DataType == VariableDataType.Date
                        ? DateTime.Now
                        : null),
            };
            this._variableDataValueRepository.Add(value);
        }

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
        var request = JsonConvert.DeserializeObject<EditSetAttributeRequest>(serializedRequest);
        var set = this._variableDataSetRepository.GetSetWithAttributesAndEntities(request.SetId);
        if (set is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var attribute = this._variableDataAttributeRepository.FindById(request.Attribute.Id);
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
        this._variableDataAttributeRepository.Update(attribute, x =>
        {
            if (!string.IsNullOrEmpty(request.Attribute.Name))
            {
                x.Name = request.Attribute.Name;
            }

            x.Label = request.Attribute.Label;
            x.HelpLabel = request.Attribute.HelpLabel;
            x.Placeholder = request.Attribute.Placeholder;
            x.Required = request.Attribute.Required;
            x.Readonly = request.Attribute.Readonly;
            x.Hidden = request.Attribute.Hidden;
            x.ShowInGrid = request.Attribute.ShowInGrid;
        });
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
        var request = JsonConvert.DeserializeObject<DeleteSetAttributeRequest>(serializedRequest);
        var set = this._variableDataSetRepository.GetSetWithAttributesAndEntities(request.SetId);
        if (set is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var attributeToDelete = this._variableDataAttributeRepository.FindById(request.AttributeId);
        if (attributeToDelete is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        foreach (var entity in set.Entities)
        {
            foreach (var value in entity.Values.Where(value => value.Attribute.Id == request.AttributeId))
            {
                this._variableDataValueRepository.Delete(value.Id);
            }
        }

        var attributeName = attributeToDelete.Name;
        this._variableDataAttributeRepository.Delete(attributeToDelete.Id);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.DELETE",
            "LABEL.ATTRIBUTE.SUCCESS.DELETE",
            new Dictionary<string, string>
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

        var entities = this.GetEntitiesPage(setType, request.Page);
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

        var entities = this.GetEntities(setType, true, request.Filter);
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

        var entityToDelete = this._variableDataEntityRepository.FindById(request.Id);
        if (entityToDelete is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._variableDataEntityRepository.Delete(entityToDelete.Id);
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
        var set = this._variableDataSetRepository.GetSetWithAttributesAndEntities(setId);
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

        entityToAdd.Set = set;
        entityToAdd.SetId = set.Id;
        this._variableDataEntityRepository.Add(entityToAdd);
        var values = attributeValueMap.Select(item => new VariableDataValue
            {
                AttributeId = item.Key.Id,
                EntityId = entityToAdd.Id,
                Value = JsonConvert.SerializeObject(item.Value),
            })
            .ToList();

        foreach (var value in values)
        {
            this._variableDataValueRepository.Add(value);
        }

        this.InvokeEvent(
            VariableDataEntityObservableType.Add,
            this.FindSetTypeById(setId),
            new VariableDataEvent
            {
                Entity = this.MapEntity(entityToAdd),
                Player = FPlayer.FromPlayer(player),
            });
        return entityToAdd;
    }

    private void InternalUpdateEntity(Player player, string setId, string entityId, Dictionary<string, object> newValues)
    {
        var set = this._variableDataSetRepository.GetSetWithAttributesAndEntities(setId);
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

        var entityValues = this._variableDataValueRepository.GetValuesByEntityId(entityId);
        foreach (var value in entityValues)
        {
            this._variableDataValueRepository.Update(value, x => { x.Value = JsonConvert.SerializeObject(attributeValueMap[value.Attribute.Name]); });
        }

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
        foreach (var frame in new System.Diagnostics.StackTrace().GetFrames())
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