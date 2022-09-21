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

#pragma warning disable CS1591
#pragma warning disable SA1600
#pragma warning disable S125
#pragma warning disable SA1512
#pragma warning disable SA1515

#pragma warning disable SA1649

namespace FivePD.Gamemode.Server.Services
{
    public class LooseDataService : ILooseDataService, IServiceWithEvents
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly DatabaseContext _databaseContext;
        private readonly List<LooseDataEntityObservable> _observables = new List<LooseDataEntityObservable>();
        private Dictionary<Type, string> _activeSetTypes;

        public LooseDataService(EventHandlerDictionary eventHandlerDictionary, DatabaseContext databaseContext)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._databaseContext = databaseContext;
        }

        public IDisposable OnAdd<TE>(Action<TE> observer)
            where TE : LooseDataEntity
        {
            return this.OnEvent(LooseDataEntityObservableType.Add, observer);
        }

        public IDisposable OnEdit<TE>(Action<TE> observer)
            where TE : LooseDataEntity
        {
            return this.OnEvent(LooseDataEntityObservableType.Edit, observer);
        }

        public IDisposable OnDelete<TE>(Action<TE> observer)
            where TE : LooseDataEntity
        {
            return this.OnEvent(LooseDataEntityObservableType.Delete, observer);
        }

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

        public void RegisterSetAttributes<TS>(List<LooseDataAttribute> attributes)
            where TS : LooseDataSet
        {
            try
            {
                // TODO ? AddonMetadataService.GetExecutingAddonDefinition()
                this._activeSetTypes ??= Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => type.IsClass && type.IsSubclassOf(typeof(LooseDataSet)))
                    .ToDictionary(type => type, type => type.Assembly.GetName().Name);

                var setType = typeof(TS);
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
                                    attr.DataType == LooseDataAttribute.LooseDataType.DateTime
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

        public void AddEntity<TS, TE>(TE entityToAdd)
            where TS : LooseDataSet
            where TE : LooseDataEntity
        {
            try
            {
                var setType = typeof(TS);
                var entityType = typeof(TE);
                var setIdAttr = setType.GetCustomAttribute<LooseDataSetIdAttribute>();
                if (setIdAttr is null)
                {
                    return;
                }

                this.InternalAddEntity(setIdAttr.Id, entityType, entityToAdd);
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        public void UpdateEntity<TE>(TE entityToUpdate)
            where TE : LooseDataEntity
        {
            try
            {
                var type = typeof(TE);
                var originalEntity = this.GetEntity<TE>(entityToUpdate.Id);

                this.InternalUpdateEntity(originalEntity.SetId, type, originalEntity, entityToUpdate);
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        public TE GetEntity<TE>(int id)
            where TE : LooseDataEntity
        {
            return this.GetEntityWithOptionalType<TE>(null, id);
        }

        public List<TE> GetEntities<TS, TE>()
            where TS : LooseDataSet
            where TE : LooseDataEntity
        {
            return this.GetEntitiesWithOptionalType<TS, TE>(null, null);
        }

        public void DeleteEntity<TE>(TE entityToDelete)
            where TE : LooseDataEntity
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
                this.InvokeEvent(LooseDataEntityObservableType.Delete, this.MapEntityWithOptionalType<TE>(null, entity));
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private TE GetEntityWithOptionalType<TE>(Type type, int id)
            where TE : LooseDataEntity
        {
            try
            {
                var entity = this._databaseContext.LooseDataEntities
                    .Include(entity => entity.Values)
                    .ThenInclude(values => values.Attribute)
                    .Include(entity => entity.Set)
                    .ThenInclude(set => set.Attributes)
                    .FirstOrDefault(entity => entity.Id == id);

                var mappedEntity = entity is null ? default : this.MapEntityWithOptionalType<TE>(type, entity);
                return mappedEntity;
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private List<TE> GetEntitiesWithOptionalType<TS, TE>(Type setType, Type entityType)
            where TS : LooseDataSet
            where TE : LooseDataEntity
        {
            try
            {
                var setIdAttr = (setType ?? typeof(TS)).GetCustomAttribute<LooseDataSetIdAttribute>();
                if (setIdAttr is null)
                {
                    return new List<TE>();
                }

                var entities = this._databaseContext.LooseDataEntities
                    .Include(entity => entity.Values)
                    .Include(entity => entity.Set)
                    .ThenInclude(set => set.Attributes)
                    .Where(entity => entity.Set.Id == setIdAttr.Id)
                    .ToList();

                return entities.Select(entity => this.MapEntityWithOptionalType<TE>(entityType, entity)).ToList();
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private Page<LooseDataEntity> GetEntitiesPage<TS, TE>(Type setType, Type entityType, int pageNumber, Filters<LooseDataEntity> filters)
            where TS : LooseDataSet
            where TE : LooseDataEntity
        {
            try
            {
                var setIdAttr = (setType ?? typeof(TS)).GetCustomAttribute<LooseDataSetIdAttribute>();
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

                page.Items = page.Items.Select(entity => this.MapEntityWithOptionalType<TE>(entityType, entity)).ToList();
                return page;
            }
            catch (Exception)
            {
                return new Page<LooseDataEntity>();
            }
        }

        private TE MapEntityWithOptionalType<TE>(Type entityType, LooseDataEntity entity)
            where TE : LooseDataEntity
        {
            if (entity.Set?.Attributes is null || entity.Values is null)
            {
                return default;
            }

            var attributeValueMap = new Dictionary<string, object>();
            foreach (var attr in entity.Set.Attributes)
            {
                attributeValueMap[attr.Name] = null;
            }

            foreach (var value in entity.Values.Where(a => this._activeSetTypes.ContainsValue(a.Attribute.AddonId)))
            {
                attributeValueMap[value.Attribute.Name] = JsonConvert.DeserializeObject(value.Value);
            }

            attributeValueMap["Id"] = entity.Id;
            attributeValueMap["SetId"] = entity.SetId;

            var mappedEntity = entityType is null ? Activator.CreateInstance<TE>() : Activator.CreateInstance(entityType);
            var mappedEntityType = mappedEntity.GetType();
            foreach (var item in attributeValueMap)
            {
                if (item.Value is null)
                {
                    continue;
                }

                var propertyInfo = mappedEntityType.GetProperty(item.Key);
                if (propertyInfo is null)
                {
                    continue;
                }

                var value = propertyInfo.PropertyType.IsEnum
                    ? Enum.Parse(propertyInfo.PropertyType, item.Value.ToString())
                    : Convert.ChangeType(item.Value, propertyInfo.PropertyType);
                propertyInfo.SetValue(mappedEntity, value, null);
            }

            if (attributeValueMap.ContainsKey("DepartmentId"))
            {
                var propertyInfo = mappedEntityType.GetProperty("Department");
                if (propertyInfo != null)
                {
                    var item = this._databaseContext.Departments.FirstOrDefault(dept => dept.Id == (attributeValueMap["DepartmentId"] as long? ?? 0));
                    propertyInfo.SetValue(mappedEntity, Convert.ChangeType(item, propertyInfo.PropertyType), null);
                }
            }

            return (TE)mappedEntity;
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
                            request.Attribute.DataType == LooseDataAttribute.LooseDataType.DateTime
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
                var types = this.FindSetTypesFromId(request.SetId);
                if (types?.Item1 is null || types.Item2 is null)
                {
                    networkCallbackDelegate.Invoke(null);
                    return;
                }

                var entity = this.GetEntityWithOptionalType<LooseDataEntity>(types.Item2, request.Id);
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
                var types = this.FindSetTypesFromId(request.SetId);
                if (types?.Item1 is null || types.Item2 is null)
                {
                    networkCallbackDelegate.Invoke(null);
                    return;
                }

                var filters = new Filters<LooseDataEntity>();
                var entities = this.GetEntitiesPage<LooseDataSet, LooseDataEntity>(types.Item1, types.Item2, request.Page, filters);
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
                var types = this.FindSetTypesFromId(request.SetId);
                if (types?.Item1 is null || types.Item2 is null)
                {
                    networkCallbackDelegate.InvokeWithUnexpectedError();
                    return;
                }

                var entityToAdd = request.Entity.ConvertTo<LooseDataEntity>(types.Item2);
                var addedEntity = this.InternalAddEntity(request.SetId, types.Item2, entityToAdd);
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
                var types = this.FindSetTypesFromId(request.SetId);
                if (types?.Item1 is null || types.Item2 is null)
                {
                    networkCallbackDelegate.InvokeWithUnexpectedError();
                    return;
                }

                var entityToUpdate = request.Entity.ConvertTo<LooseDataEntity>(types.Item2);
                var originalEntity = this.GetEntityWithOptionalType<LooseDataEntity>(types.Item2, entityToUpdate.Id);
                this.InternalUpdateEntity(request.SetId, types.Item2, originalEntity, entityToUpdate);
                networkCallbackDelegate.InvokeWithResponse(true, "Nui.Labels.SuccessfulEdit", "Nui.Label.SuccessfulEntityEdit");
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
                var types = this.FindSetTypesFromId(request.SetId);
                if (types?.Item1 is null || types.Item2 is null)
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
                this.InvokeEvent(LooseDataEntityObservableType.Delete, entityToDelete);
                networkCallbackDelegate.InvokeWithResponse(true, "Nui.Labels.SuccessfulDelete", "Nui.Labels.SuccessfulEntityDelete");
            }
            catch (Exception)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
            }
        }

        private LooseDataEntity InternalAddEntity<TE>(string setId, Type entityType, TE entityToAdd)
            where TE : LooseDataEntity
        {
            var foundSet = this._databaseContext.LooseDataSets
                .Include(s => s.Attributes)
                .FirstOrDefault(s => s.Id == setId);
            if (foundSet is null)
            {
                return null;
            }

            var attributeValueMap = new Dictionary<LooseDataAttribute, object>();
            foreach (var attr in foundSet.Attributes)
            {
                attributeValueMap[attr] = null;
            }

            var properties = entityType.GetProperties();
            foreach (var property in properties)
            {
                var attribute = foundSet.Attributes.FirstOrDefault(a => a.Name == property.Name);
                if (attribute is null)
                {
                    continue;
                }

                attributeValueMap[attribute] = property.GetValue(entityToAdd, null);
            }

            var entity = new LooseDataEntity
            {
                Set = foundSet,
            };
            var values = attributeValueMap.Select(item => new LooseDataValue
                {
                    Attribute = item.Key,
                    Entity = entity,
                    Value = JsonConvert.SerializeObject(item.Value),
                })
                .ToList();

            this._databaseContext.LooseDataValues.AddRange(values);
            var entityEntry = this._databaseContext.LooseDataEntities.Add(entity);
            this._databaseContext.SaveChanges();
            this.InvokeEvent(LooseDataEntityObservableType.Add, this.MapEntityWithOptionalType<LooseDataEntity>(entityType, entityEntry.Entity));
            return entityEntry.Entity;
        }

        private void InternalUpdateEntity<TE>(string setId, Type entityType, TE originalEntity, TE entityToUpdate)
            where TE : LooseDataEntity
        {
            var set = this._databaseContext.LooseDataSets.FirstOrDefault(set => set.Id == setId);
            if (set is null)
            {
                return;
            }

            var attributeValueMap = new Dictionary<string, object>();
            foreach (var name in set.Attributes.Select(attr => attr.Name))
            {
                attributeValueMap[name] = entityType.GetProperty(name)?.GetValue(originalEntity);
            }

            var properties = entityType.GetProperties();
            foreach (var property in properties)
            {
                var attribute = set.Attributes.FirstOrDefault(a => a.Name == property.Name);
                if (attribute is null)
                {
                    continue;
                }

                attributeValueMap[attribute.Name] = property.GetValue(entityToUpdate, null);
            }

            var entityValues = this._databaseContext.LooseDataValues
                .Include(value => value.Entity)
                .Where(value => value.Entity.Id == originalEntity.Id)
                .ToList();
            foreach (var dataValue in entityValues)
            {
                dataValue.Value = JsonConvert.SerializeObject(attributeValueMap[dataValue.Attribute.Name]);
            }

            this._databaseContext.SaveChanges();
            this.InvokeEvent(LooseDataEntityObservableType.Edit, this.GetEntityWithOptionalType<LooseDataEntity>(entityType, entityToUpdate.Id));
        }

        /// <summary>
        /// Finds the set and entity types from the set's id.
        /// </summary>
        /// <param name="setId">The id of the given set.</param>
        /// <returns>A tuple containing the set's and the entity's type in this order.</returns>
        private Tuple<Type, Type> FindSetTypesFromId(string setId)
        {
            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            var setType = assemblyTypes
                .Where(type => type.IsClass)
                .Where(type => type.GetCustomAttribute<LooseDataSetIdAttribute>() != null)
                .FirstOrDefault(type => type.GetCustomAttribute<LooseDataSetIdAttribute>().Id == setId);

            if (setType is null)
            {
                return new Tuple<Type, Type>(null, null);
            }

            var entityType = assemblyTypes
                .Where(type => type.IsClass)
                .FirstOrDefault(type => type.Name == setType.GetCustomAttribute<LooseDataEntityModelAttribute>().ClassName);

            return new Tuple<Type, Type>(setType, entityType);
        }

        private IDisposable OnEvent<TE>(LooseDataEntityObservableType observableType, Action<TE> observer)
            where TE : LooseDataEntity
        {
            var type = typeof(TE);
            var observable = this._observables.FirstOrDefault(o => o.EntityType == type && o.Type == observableType);
            if (observable is null)
            {
                observable = new LooseDataEntityObservable(type, observableType);
                this._observables.Add(observable);
            }

            return observable.Subscribe(entity =>
            {
                observer?.Invoke(entity as TE);
            });
        }

        private void InvokeEvent<TE>(LooseDataEntityObservableType observableType, TE entity)
            where TE : LooseDataEntity
        {
            var observable = this._observables.FirstOrDefault(o => o.Type == observableType && o.EntityType == typeof(TE));
            observable?.InvokeObservers(entity);
        }
    }
}