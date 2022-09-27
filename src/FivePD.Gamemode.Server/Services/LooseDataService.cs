// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Server.Attributes;
using FivePD.Gamemode.Server.Database;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models.LooseDataSet;
using FivePD.Gamemode.Server.Models.Requests;
using FivePD.Gamemode.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.ILooseDataService" />
    public class LooseDataService : ILooseDataService, IServiceWithEvents
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly DatabaseContext _databaseContext;
        private readonly List<LooseDataEntityObservable> _observables = new List<LooseDataEntityObservable>();
        private readonly Dictionary<Type, string> _activeSetTypes = new Dictionary<Type, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LooseDataService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
        /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
        public LooseDataService(EventHandlerDictionary eventHandlerDictionary, DatabaseContext databaseContext)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
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
        public void RegisterEvents()
        {
            this._eventHandlerDictionary[Events.LooseData.GetEntity] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetEntityFromNui);
            this._eventHandlerDictionary[Events.LooseData.GetEntities] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetEntitiesFromNui);
            this._eventHandlerDictionary[Events.LooseData.AddEntity] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.AddEntityFromNui);
            this._eventHandlerDictionary[Events.LooseData.EditEntity] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.UpdateEntityFromNui);
            this._eventHandlerDictionary[Events.LooseData.DeleteEntity] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.DeleteEntityFromNui);
            this._eventHandlerDictionary[Events.LooseData.GetSetAttributes] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetSetAttributesFromNui);
            this._eventHandlerDictionary[Events.LooseData.GetSetAttributesPageable] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetSetAttributesPageableFromNui);
            this._eventHandlerDictionary[Events.LooseData.AddSetAttribute] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.AddSetAttributeFromNui);
            this._eventHandlerDictionary[Events.LooseData.EditSetAttribute] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.EditSetAttributeFromNui);
            this._eventHandlerDictionary[Events.LooseData.DeleteSetAttribute] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.DeleteSetAttributeFromNui);
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
                    assemblyName = "FivePD Gamemode";
                }

                var setType = typeof(TS);
                this._activeSetTypes[setType] = assemblyName;

                var setIdAttr = setType.GetCustomAttribute<LooseDataSetIdAttribute>();
                if (setIdAttr is null)
                {
                    return;
                }

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
                                    : (object)null),
                            };
                            entity.Values.Add(value);
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
            return this.GetEntityWithOptionalType(id);
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
                this.InvokeEvent(LooseDataEntityObservableType.Delete, this.FindSetTypeById(entity.Set.Id), this.MapEntityWithOptionalType(entity));
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private LooseDataEntity GetEntityWithOptionalType(int id)
        {
            try
            {
                var entity = this._databaseContext.LooseDataEntities
                    .Include(entity => entity.Values)
                    .ThenInclude(values => values.Attribute)
                    .Include(entity => entity.Set)
                    .ThenInclude(set => set.Attributes)
                    .FirstOrDefault(entity => entity.Id == id);

                var mappedEntity = entity is null ? null : this.MapEntityWithOptionalType(entity);
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

                return entities.Select(this.MapEntityWithOptionalType).ToList();
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

                page.Items = page.Items.Select(this.MapEntityWithOptionalType).ToList();
                return page;
            }
            catch (Exception)
            {
                return new Page<LooseDataEntity>();
            }
        }

        private LooseDataEntity MapEntityWithOptionalType(LooseDataEntity entity)
        {
            if (entity.Set?.Attributes is null || entity.Values is null)
            {
                return default;
            }

            entity.MappedValues = new Dictionary<string, object>();
            foreach (var value in entity.Values.Where(a => a.Attribute.AddonId == LooseDataAttribute.AddedByAdminAddonId || this._activeSetTypes.ContainsValue(a.Attribute.AddonId)))
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
                    networkCallbackDelegate.InvokeWithResponse(false, "Nui.Errors.CreateAttributeExistsWithNameTitle", "Nui.Errors.AttributeExistsWithNameDescription");
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
                                : (object)null),
                    };
                    entity.Values.Add(value);
                }

                networkCallbackDelegate.InvokeWithResponse(true, "Nui.Labels.SuccessfulCreate", "Nui.Labels.SuccessfulAttributeCreate", new Dictionary<string, string>
                {
                    { "attribute", request.Attribute.Name },
                });
            }
            catch (Exception)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
            }
        }

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

                if (set.Attributes.Any(attr => attr.Name == request.Attribute.Name))
                {
                    networkCallbackDelegate.InvokeWithResponse(false, "Nui.Errors.CreateAttributeExistsWithNameTitle", "Nui.Errors.AttributeExistsWithNameDescription");
                    return;
                }

                var attribute = this._databaseContext.LooseDataAttributes
                    .FirstOrDefault(attr => attr.Id == request.Attribute.Id);
                if (attribute is null)
                {
                    networkCallbackDelegate.InvokeWithUnexpectedError();
                    return;
                }

                var attributeOldName = attribute.Name;
                attribute.Name = request.Attribute.Name;
                attribute.Label = request.Attribute.Label;
                attribute.HelpLabel = request.Attribute.HelpLabel;
                attribute.Placeholder = request.Attribute.Placeholder;
                attribute.Required = request.Attribute.Required;
                this._databaseContext.SaveChanges();
                networkCallbackDelegate.InvokeWithResponse(true, "Nui.Labels.SuccessfulEdit", "Nui.Labels.SuccessfulAttributeEdit", new Dictionary<string, string>
                {
                    { "attribute", attributeOldName },
                });
            }
            catch (Exception)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
            }
        }

        private void DeleteSetAttributeFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var request = expandoObject.ConvertTo<DeleteSetAttributeRequest>();
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
                networkCallbackDelegate.InvokeWithResponse(true, "Nui.Labels.SuccessfulDelete", "Nui.Labels.SuccessfulAttributeDelete", new Dictionary<string, string>
                {
                    { "attribute", attributeName },
                });
            }
            catch (Exception)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
            }
        }

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

                var entity = this.GetEntityWithOptionalType(request.Id);
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(entity));
            }
            catch (Exception)
            {
                networkCallbackDelegate.Invoke(null);
            }
        }

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

                networkCallbackDelegate.InvokeWithResponse(true, "Nui.Labels.SuccessfulCreate", "Nui.Label.SuccessfulEntityCreate");
            }
            catch (Exception)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
            }
        }

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
                networkCallbackDelegate.InvokeWithResponse(true, "Nui.Labels.SuccessfulEdit", "Nui.Labels.SuccessfulEntityEdit");
            }
            catch (Exception)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
            }
        }

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
                networkCallbackDelegate.InvokeWithResponse(true, "Nui.Labels.SuccessfulDelete", "Nui.Labels.SuccessfulEntityDelete");
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
            this.InvokeEvent(LooseDataEntityObservableType.Add, this.FindSetTypeById(setId), this.MapEntityWithOptionalType(entityEntry.Entity));
            return entityEntry.Entity;
        }

        private void InternalUpdateEntity(string setId, int entityId, Dictionary<string, object> newValues)
        {
            var set = this._databaseContext.LooseDataSets.FirstOrDefault(set => set.Id == setId);
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
                .Where(value => value.Entity.Id == entityId)
                .ToList();
            foreach (var dataValue in entityValues)
            {
                dataValue.Value = JsonConvert.SerializeObject(attributeValueMap[dataValue.Attribute.Name]);
            }

            this._databaseContext.SaveChanges();
            this.InvokeEvent(LooseDataEntityObservableType.Edit, this.FindSetTypeById(setId), this.GetEntityWithOptionalType(entityId));
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

            return observable.Subscribe(entity =>
            {
                observer?.Invoke(entity);
            });
        }

        private void InvokeEvent(LooseDataEntityObservableType observableType, Type setType, LooseDataEntity entity)
        {
            var observable = this._observables.FirstOrDefault(o => o.Type == observableType && o.SetType == setType);
            observable?.InvokeObservers(entity);
        }

        private string GetExecutingAssemblyName()
        {
            // Technically, the executing assembly is the server gamemode. Sort through the stack trace and find a ref. to the actual addon
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
}