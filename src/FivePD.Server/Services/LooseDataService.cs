using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Exceptions;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.LooseDataSet;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class LooseDataService : ILooseDataService
{
    private readonly DatabaseContext _databaseContext;
    private readonly List<LooseDataEntityObservable> _observables = new();
    private readonly Dictionary<Type, string> _activeSetTypes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LooseDataService"/> class.
    /// </summary>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    public LooseDataService(DatabaseContext databaseContext)
    {
        this._databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public IDisposable OnAdd<TS>(Action<LooseDataEntity> observer)
    {
        return this.OnEvent<TS>(LooseDataEntityObservableType.Add, observer);
    }

    /// <inheritdoc />
    public IDisposable OnEdit<TS>(Action<LooseDataEntity> observer)
    {
        return this.OnEvent<TS>(LooseDataEntityObservableType.Edit, observer);
    }

    /// <inheritdoc />
    public IDisposable OnDelete<TS>(Action<LooseDataEntity> observer)
    {
        return this.OnEvent<TS>(LooseDataEntityObservableType.Delete, observer);
    }

    /// <inheritdoc />
    public void RegisterSetAttributes<TS>(List<LooseDataAttribute> attributes)
        where TS : LooseDataSet
    {
        try
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

            var setIdAttr = setType.GetCustomAttribute<LooseDataSetIdAttribute>();
            if (setIdAttr is null)
            {
                return;
            }

#pragma warning disable
            // TODO: add check for official set names (e.g. fivepd_arrest_report)
            // TODO: add check for duplicate attributes
#pragma warning enable

            var set = this._databaseContext.LooseDataSets
                .Include(set => set.Attributes)
                .Include(set => set.Entities)
                .ThenInclude(entities => entities.Values)
                .FirstOrDefault(set => set.Id == setIdAttr.Id);
            if (set is null)
            {
                set = Activator.CreateInstance<TS>();
                set.Id = setIdAttr.Id;
                this._databaseContext.LooseDataSets.Add(set);
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
                    var addedAttributeEntry = this._databaseContext.LooseDataAttributes.Add(attr);
                    this._databaseContext.SaveChanges();
                    foreach (var entity in set.Entities)
                    {
                        var value = new LooseDataValue
                        {
                            Entity = entity,
                            Attribute = addedAttributeEntry.Entity,
                            Value = JsonConvert.SerializeObject(
                                attr.DataType == LooseDataAttribute.LooseDataType.Date
                                    ? DateTime.Now
                                    : null),
                        };
                        this._databaseContext.LooseDataValues.Add(value);
                    }
                }
            }
            else
            {
                set.Attributes = attributes;
            }

            this._databaseContext.SaveChanges();
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    /// <inheritdoc />
    public void UpdateSetAttribute<TS>(string attributeName, LooseDataAttribute newAttributeValues)
        where TS : LooseDataSet
    {
        try
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

            var setIdAttr = setType.GetCustomAttribute<LooseDataSetIdAttribute>();
            if (setIdAttr is null)
            {
                return;
            }

            var attributeToUpdate = this._databaseContext.LooseDataAttributes
                .FirstOrDefault(attr => attr.Name == attributeName);
            if (attributeToUpdate is null)
            {
                return;
            }

            if (attributeToUpdate.AddonId == "FivePD Gamemode" || attributeToUpdate.AddonId != assemblyName)
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
            this._databaseContext.SaveChanges();
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    /// <inheritdoc />
    public void DeleteSetAttribute<TS>(string attributeName)
        where TS : LooseDataSet
    {
        try
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

            var setIdAttr = setType.GetCustomAttribute<LooseDataSetIdAttribute>();
            if (setIdAttr is null)
            {
                return;
            }

            var attributeToDelete = this._databaseContext.LooseDataAttributes
                .FirstOrDefault(attr => attr.Name == attributeName);
            if (attributeToDelete is null)
            {
                return;
            }

            if (attributeToDelete.AddonId == "FivePD Gamemode" || attributeToDelete.AddonId != assemblyName)
            {
                throw new ArgumentException("Can't delete attribute of a different addon");
            }

            var valuesToDelete = this._databaseContext.LooseDataValues
                .Include(value => value.Attribute)
                .Where(value => value.Attribute.Id == attributeToDelete.Id);
            this._databaseContext.LooseDataValues.RemoveRange(valuesToDelete);
            this._databaseContext.LooseDataAttributes.Remove(attributeToDelete);
            this._databaseContext.SaveChanges();
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    /// <inheritdoc />
    public void AddEntity<TS>(LooseDataEntity entityToAdd)
        where TS : LooseDataSet
    {
        try
        {
            var setType = typeof(TS);
            var setIdAttr = setType.GetCustomAttribute<LooseDataSetIdAttribute>();
            if (setIdAttr is null)
            {
                return;
            }

            this.InternalAddEntity(setIdAttr.Id, entityToAdd, entityToAdd.MappedValues);
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    /// <inheritdoc />
    public void UpdateEntity(LooseDataEntity entityToUpdate)
    {
        try
        {
            var originalEntity = this.GetEntity(entityToUpdate.Id);
            if (originalEntity is null)
            {
                throw new ArgumentException("No entity exist with the given id");
            }

            this.InternalUpdateEntity(originalEntity.SetId, originalEntity.Id, entityToUpdate.MappedValues);
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    /// <inheritdoc />
    public LooseDataEntity GetEntity(int id)
    {
        return this.InternalGetEntity(id);
    }

    /// <inheritdoc />
    public List<LooseDataEntity> GetEntities<TS>()
        where TS : LooseDataSet
    {
        return this.GetEntitiesWithOptionalType<TS>(null);
    }

    /// <inheritdoc />
    public void DeleteEntity(LooseDataEntity entityToDelete)
    {
        try
        {
            var entity = this._databaseContext.LooseDataEntities
                .Include(entity => entity.Values)
                .Include(entity => entity.Set)
                .FirstOrDefault(entity => entity.Id == entityToDelete.Id);
            if (entity is null)
            {
                return;
            }

            this._databaseContext.LooseDataEntities.Remove(entity);
            this._databaseContext.SaveChanges();
            this.InvokeEvent(LooseDataEntityObservableType.Delete, this.FindSetTypeById(entity.Set.Id), this.MapEntity(entity));
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    private LooseDataEntity InternalGetEntity(int id)
    {
        try
        {
            var entity = this._databaseContext.LooseDataEntities
                .Include(entity => entity.Values)
                .ThenInclude(values => values.Attribute)
                .Include(entity => entity.Set)
                .ThenInclude(set => set.Attributes)
                .FirstOrDefault(entity => entity.Id == id);

            var mappedEntity = entity is null ? null : this.MapEntity(entity);
            return mappedEntity;
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    private List<LooseDataEntity> GetEntitiesWithOptionalType<TS>(Type setType)
        where TS : LooseDataSet
    {
        try
        {
            var setIdAttr = (setType ?? typeof(TS)).GetCustomAttribute<LooseDataSetIdAttribute>();
            if (setIdAttr is null)
            {
                return new List<LooseDataEntity>();
            }

            var entities = this._databaseContext.LooseDataEntities
                .Include(entity => entity.Values)
                .Include(entity => entity.Set)
                .ThenInclude(set => set.Attributes)
                .Where(entity => entity.Set.Id == setIdAttr.Id)
                .ToList();

            return entities.Select(this.MapEntity).ToList();
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    private Page<LooseDataEntity> GetEntitiesPage(Type setType, int pageNumber, Filters<LooseDataEntity> filters)
    {
        try
        {
            var setIdAttr = setType.GetCustomAttribute<LooseDataSetIdAttribute>();
            if (setIdAttr is null)
            {
                return new Page<LooseDataEntity>();
            }

            var page = this._databaseContext.LooseDataEntities
                .Include(entity => entity.Values)
                .Include(entity => entity.Set)
                .ThenInclude(set => set.Attributes)
                .Where(entity => entity.Set.Id == setIdAttr.Id)
                .Paginate(pageNumber, filters);

            page.Items = page.Items.Select(this.MapEntity).ToList();
            return page;
        }
        catch (Exception)
        {
            return new Page<LooseDataEntity>();
        }
    }

    private LooseDataEntity MapEntity(LooseDataEntity entity)
    {
        if (entity.Set?.Attributes is null || entity.Values is null)
        {
            return default;
        }

        entity.MappedValues = new Dictionary<string, object>();
        foreach (var value in entity.Values.Where(a =>
                     a.Attribute.AddonId == LooseDataAttribute.AddedByAdminAddonId || this._activeSetTypes.ContainsValue(a.Attribute.AddonId)))
        {
            var deserializedValue = JsonConvert.DeserializeObject(value.Value);
            entity.MappedValues[value.Attribute.Name] = deserializedValue;
        }

        if (entity.MappedValues.ContainsKey("DepartmentId"))
        {
            var department = this._databaseContext.Departments.FirstOrDefault(dept => dept.Id == (entity.MappedValues["DepartmentId"] as long? ?? 0));
            entity.MappedValues["Department"] = department;
        }

        if (entity.MappedValues.ContainsKey("UserId"))
        {
            var user = this._databaseContext.Users.FirstOrDefault(user => user.Id == (entity.MappedValues["UserId"] as long? ?? 0));
            entity.MappedValues["User"] = user;
        }

        return entity;
    }

    [UIEvent(Events.UI.LooseData.GetSetAttribute)]
    private void GetSetAttributeFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<IdRequest>();
            var attribute = this._databaseContext.LooseDataAttributes
                .AsNoTracking()
                .FirstOrDefault(set => set.Id == request.Id);

            if (attribute is null)
            {
                networkCallbackDelegate.Invoke(null);
                return;
            }

            networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(attribute));
        }
        catch (Exception)
        {
            networkCallbackDelegate.Invoke(null);
        }
    }

    [UIEvent(Events.UI.LooseData.GetSetAttributes)]
    private void GetSetAttributesFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<GetSetRequest>();
            var set = this._databaseContext.LooseDataSets
                .Include(set => set.Attributes)
                .FirstOrDefault(set => set.Id == request.SetId);

            if (set is null)
            {
                networkCallbackDelegate.Invoke(null);
                return;
            }

            networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(set.Attributes));
        }
        catch (Exception)
        {
            networkCallbackDelegate.Invoke(null);
        }
    }

    [UIEvent(Events.UI.LooseData.GetSetAttributesPageable)]
    private void GetSetAttributesPageableFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<GetSetRequest>();
            var page = this._databaseContext.LooseDataAttributes
                .AsNoTracking()
                .Include(attr => attr.Set)
                .Where(attr => attr.Set.Id == request.SetId)
                .Paginate(request.Page, new Filters<LooseDataAttribute>());

            networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(page));
        }
        catch (Exception)
        {
            networkCallbackDelegate.Invoke(null);
        }
    }

    [UIEvent(Events.UI.LooseData.AddSetAttribute)]
    private void AddSetAttributeFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<AddSetAttributeRequest>();
            var set = this._databaseContext.LooseDataSets
                .Include(set => set.Attributes)
                .Include(set => set.Entities)
                .ThenInclude(entities => entities.Values)
                .FirstOrDefault(set => set.Id == request.SetId);
            if (set is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            if (set.Attributes.Any(attr => attr.Name == request.Attribute.Name))
            {
                networkCallbackDelegate.InvokeWithResponse(false, "UI.ERROR.ATTRIBUTE.CREATE.TITLE", "UI.ERROR.ATTRIBUTE.NAME_CONFLICT.DESCRIPTION");
                return;
            }

            request.Attribute.AddonId = LooseDataAttribute.AddedByAdminAddonId;
            request.Attribute.Set = set;
            var addedAttributeEntry = this._databaseContext.LooseDataAttributes.Add(request.Attribute);
            this._databaseContext.SaveChanges();

            foreach (var entity in set.Entities)
            {
                var value = new LooseDataValue
                {
                    Entity = entity,
                    Attribute = addedAttributeEntry.Entity,
                    Value = JsonConvert.SerializeObject(
                        request.Attribute.DataType == LooseDataAttribute.LooseDataType.Date
                            ? DateTime.Now
                            : null),
                };
                this._databaseContext.LooseDataValues.Add(value);
            }

            this._databaseContext.SaveChanges();
            networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.CREATE", "UI.LABEL.ATTRIBUTE.SUCCESS.CREATE", new Dictionary<string, string>
            {
                { "attribute", request.Attribute.Name },
            });
        }
        catch (Exception)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
        }
    }

    [UIEvent(Events.UI.LooseData.EditSetAttribute)]
    private void EditSetAttributeFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<EditSetAttributeRequest>();

            var set = this._databaseContext.LooseDataSets
                .Include(set => set.Attributes)
                .FirstOrDefault(set => set.Id == request.SetId);
            if (set is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            var attribute = this._databaseContext.LooseDataAttributes.FirstOrDefault(attr => attr.Id == request.Attribute.Id);
            if (attribute is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            if (set.Attributes.Where(attr => attr.Id != attribute.Id).Any(attr => attr.Name == request.Attribute.Name))
            {
                networkCallbackDelegate.InvokeWithResponse(false, "UI.ERROR.ATTRIBUTE.EDIT.TITLE", "UI.ERROR.ATTRIBUTE.NAME_CONFLICT.DESCRIPTION");
                return;
            }


            var attributeOldName = attribute.Name;
            attribute.Name = request.Attribute.Name;
            attribute.Label = request.Attribute.Label;
            attribute.HelpLabel = request.Attribute.HelpLabel;
            attribute.Placeholder = request.Attribute.Placeholder;
            attribute.Required = request.Attribute.Required;
            this._databaseContext.SaveChanges();
            networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.EDIT", "UI.LABEL.ATTRIBUTE.SUCCESS.EDIT", new Dictionary<string, string>
            {
                { "attribute", attributeOldName },
            });
        }
        catch (Exception)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
        }
    }

    [UIEvent(Events.UI.LooseData.DeleteSetAttribute)]
    private void DeleteSetAttributeFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<DeleteSetAttributeRequest>();
            var set = this._databaseContext.LooseDataSets
                .Include(set => set.Attributes)
                .Include(set => set.Entities)
                .ThenInclude(entities => entities.Values)
                .ThenInclude(looseDataValue => looseDataValue.Attribute)
                .FirstOrDefault(set => set.Id == request.SetId);
            if (set is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            var attributeToDelete = this._databaseContext.LooseDataAttributes
                .FirstOrDefault(attr => attr.Id == request.AttributeId);
            if (attributeToDelete is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            foreach (var entity in set.Entities)
            {
                foreach (var value in entity.Values.Where(value => value.Attribute.Id == request.AttributeId))
                {
                    this._databaseContext.LooseDataValues.Remove(value);
                }
            }

            var attributeName = attributeToDelete.Name;
            this._databaseContext.LooseDataAttributes.Remove(attributeToDelete);
            this._databaseContext.SaveChanges();
            networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.DELETE", "UI.LABEL.ATTRIBUTE.SUCCESS.DELETE", new Dictionary<string, string>
            {
                { "attribute", attributeName },
            });
        }
        catch (Exception)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
        }
    }

    [UIEvent(Events.UI.LooseData.GetEntity)]
    private void GetEntityFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<GetSetEntityRequest>();
            var setType = this.FindSetTypeById(request.SetId);
            if (setType is null)
            {
                networkCallbackDelegate.Invoke(null);
                return;
            }

            var entity = this.InternalGetEntity(request.Id);
            networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(entity));
        }
        catch (Exception)
        {
            networkCallbackDelegate.Invoke(null);
        }
    }

    [UIEvent(Events.UI.LooseData.GetEntities)]
    private void GetEntitiesFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<GetSetEntitiesRequest>();
            var setType = this.FindSetTypeById(request.SetId);
            if (setType is null)
            {
                networkCallbackDelegate.Invoke(null);
                return;
            }

            var filters = new Filters<LooseDataEntity>();
            var entities = this.GetEntitiesPage(setType, request.Page, filters);
            networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(entities));
        }
        catch (Exception)
        {
            networkCallbackDelegate.Invoke(null);
        }
    }

    [UIEvent(Events.UI.LooseData.AddEntity)]
    private void AddEntityFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<AddEntityRequest>();
            var setType = this.FindSetTypeById(request.SetId);
            if (setType is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            var addedEntity = this.InternalAddEntity(request.SetId, new LooseDataEntity(), new Dictionary<string, object>(request.Entity));
            if (addedEntity is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.DELETE", "UI.LABEL.ENTITY.SUCCESS.DELETE");
        }
        catch (Exception)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
        }
    }

    [UIEvent(Events.UI.LooseData.EditEntity)]
    private void UpdateEntityFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<AddEntityRequest>();
            var setType = this.FindSetTypeById(request.SetId);
            if (setType is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            this.InternalUpdateEntity(request.SetId, request.EntityId, new Dictionary<string, object>(request.Entity));
            networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.EDIT", "UI.LABEL.ENTITY.SUCCESS.EDIT");
        }
        catch (Exception)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
        }
    }

    [UIEvent(Events.UI.LooseData.DeleteEntity)]
    private void DeleteEntityFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<DeleteEntityRequest>();
            var setType = this.FindSetTypeById(request.SetId);
            if (setType is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            var entityToDelete = this._databaseContext.LooseDataEntities
                .FirstOrDefault(entity => entity.Id == request.Id);
            if (entityToDelete is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            this._databaseContext.LooseDataEntities.Remove(entityToDelete);
            this._databaseContext.SaveChanges();
            this.InvokeEvent(LooseDataEntityObservableType.Delete, setType, entityToDelete);
            networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.DELETE", "UI.LABEL.ENTITY.SUCCESS.DELETE");
        }
        catch (Exception)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
        }
    }

    private LooseDataEntity InternalAddEntity(string setId, LooseDataEntity entityToAdd, Dictionary<string, object> valuesToAdd)
    {
        var set = this._databaseContext.LooseDataSets
            .Include(s => s.Attributes)
            .FirstOrDefault(s => s.Id == setId);
        if (set is null)
        {
            return null;
        }

        var attributeValueMap = new Dictionary<LooseDataAttribute, object>();
        foreach (var attr in set.Attributes)
        {
            attributeValueMap[attr] = null;
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

        var values = attributeValueMap.Select(item => new LooseDataValue
            {
                Attribute = item.Key,
                Entity = entityToAdd,
                Value = JsonConvert.SerializeObject(item.Value),
            })
            .ToList();

        entityToAdd.Set = set;
        this._databaseContext.LooseDataValues.AddRange(values);
        var entityEntry = this._databaseContext.LooseDataEntities.Add(entityToAdd);
        this._databaseContext.SaveChanges();
        this.InvokeEvent(LooseDataEntityObservableType.Add, this.FindSetTypeById(setId), this.MapEntity(entityEntry.Entity));
        return entityEntry.Entity;
    }

    private void InternalUpdateEntity(string setId, int entityId, Dictionary<string, object> newValues)
    {
        var set = this._databaseContext.LooseDataSets
            .Include(looseDataSet => looseDataSet.Attributes)
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

        var entityValues = this._databaseContext.LooseDataValues
            .Include(value => value.Entity)
            .Include(looseDataValue => looseDataValue.Attribute)
            .Where(value => value.Entity.Id == entityId)
            .ToList();
        foreach (var dataValue in entityValues)
        {
            dataValue.Value = JsonConvert.SerializeObject(attributeValueMap[dataValue.Attribute.Name]);
        }

        this._databaseContext.SaveChanges();
        this.InvokeEvent(LooseDataEntityObservableType.Edit, this.FindSetTypeById(setId), this.InternalGetEntity(entityId));
    }

    private Type FindSetTypeById(string setId)
    {
        var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
        var type = assemblyTypes
            .Where(type => type.IsClass)
            .Where(type => type.GetCustomAttribute<LooseDataSetIdAttribute>() != null)
            .FirstOrDefault(type => type.GetCustomAttribute<LooseDataSetIdAttribute>().Id == setId);

        return type;
    }

    private IDisposable OnEvent<TS>(LooseDataEntityObservableType observableType, Action<LooseDataEntity> observer)
    {
        var type = typeof(TS);
        var observable = this._observables.FirstOrDefault(o => o.SetType == type && o.Type == observableType);
        if (observable is null)
        {
            observable = new LooseDataEntityObservable(type, observableType);
            this._observables.Add(observable);
        }

        return observable.Subscribe(entity => { observer?.Invoke(entity); });
    }

    private void InvokeEvent(LooseDataEntityObservableType observableType, Type setType, LooseDataEntity entity)
    {
        var observable = this._observables.FirstOrDefault(o => o.Type == observableType && o.SetType == setType);
        observable?.InvokeObservers(entity);
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