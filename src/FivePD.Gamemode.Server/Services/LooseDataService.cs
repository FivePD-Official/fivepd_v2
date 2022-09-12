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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

#pragma warning disable CS1591
#pragma warning disable SA1600
#pragma warning disable S125
#pragma warning disable SA1512
#pragma warning disable SA1515

namespace FivePD.Gamemode.Server.Services
{
    public class LooseDataService : ILooseDataService, IServiceWithEvents
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly DatabaseContext _databaseContext;
        private Dictionary<Type, string> _activeSetTypes;

        public LooseDataService(EventHandlerDictionary eventHandlerDictionary, DatabaseContext databaseContext)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._databaseContext = databaseContext;
        }

        public void RegisterEvents()
        {
            this._eventHandlerDictionary[Events.LooseData.GetSets] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetSets);
            this._eventHandlerDictionary[Events.LooseData.GetEntity] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetEntityFromNui);
            this._eventHandlerDictionary[Events.LooseData.GetEntities] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetEntitiesFromNui);
        }

        public void RegisterSetAttributes<TS>(List<LooseDataAttribute> attributes)
            where TS : LooseDataSet
        {
            try
            {
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
                        set.Attributes.Add(attr);
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

                var foundSet = this._databaseContext.LooseDataSets
                    .Include(s => s.Attributes)
                    .FirstOrDefault(s => s.Id == setIdAttr.Id);
                if (foundSet is null)
                {
                    return;
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
                this._databaseContext.LooseDataEntities.Add(entity);
                this._databaseContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        public void UpdateEntity<T>(T entityToUpdate)
            where T : LooseDataEntity
        {
            try
            {
                var type = typeof(T);
                var originalEntity = this.GetEntity<T>(entityToUpdate.Id);

                var set = this._databaseContext.LooseDataSets.FirstOrDefault(set => set.Id == originalEntity.SetId);
                if (set is null)
                {
                    return;
                }

                var attributeValueMap = new Dictionary<string, object>();
                foreach (var name in set.Attributes.Select(attr => attr.Name))
                {
                    attributeValueMap[name] = type.GetProperty(name)?.GetValue(originalEntity);
                }

                var properties = type.GetProperties();
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
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        public T GetEntity<T>(int id)
            where T : LooseDataEntity
        {
            return this.GetEntityWithOptionalType<T>(null, id);
        }

        public List<TE> GetEntities<TS, TE>()
            where TS : LooseDataSet
            where TE : LooseDataEntity
        {
            return this.GetEntitiesWithOptionalType<TS, TE>(null, null);
        }

        public void DeleteEntity<TS>(int id)
            where TS : LooseDataSet
        {
            try
            {
                var setIdAttr = typeof(TS).GetCustomAttribute<LooseDataSetIdAttribute>();
                if (setIdAttr is null)
                {
                    return;
                }

                var set = this._databaseContext.LooseDataSets
                    .Include(set => set.Attributes)
                    .FirstOrDefault(set => set.Id == setIdAttr.Id);
                if (set is null)
                {
                    return;
                }

                var entity = this._databaseContext.LooseDataEntities
                    .Include(entity => entity.Values)
                    .FirstOrDefault(entity => entity.Id == id);
                if (entity is null)
                {
                    return;
                }

                this._databaseContext.LooseDataEntities.Remove(entity);
                this._databaseContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private T GetEntityWithOptionalType<T>(Type type, int id)
            where T : LooseDataEntity
        {
            try
            {
                var entity = this._databaseContext.LooseDataEntities
                    .Include(entity => entity.Values)
                    .ThenInclude(values => values.Attribute)
                    .Include(entity => entity.Set)
                    .ThenInclude(set => set.Attributes)
                    .FirstOrDefault(entity => entity.Id == id);

                var s = entity is null ? default : this.MapEntityWithOptionalType<T>(type, entity);
                return s;
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

        private T MapEntityWithOptionalType<T>(Type entityType, LooseDataEntity entity)
            where T : LooseDataEntity
        {
            if (entity.Set?.Attributes is null || entity.Values is null)
            {
                return default;
            }

            var attributeValueMap = new Dictionary<string, object>();
            foreach (var value in entity.Values.Where(a => this._activeSetTypes.ContainsValue(a.Attribute.AddonId)))
            {
                attributeValueMap[value.Attribute.Name] = JsonConvert.DeserializeObject(value.Value);
            }

            attributeValueMap["Id"] = entity.Id;
            attributeValueMap["SetId"] = entity.SetId;

            var mappedEntity = entityType is null ? Activator.CreateInstance<T>() : Activator.CreateInstance(entityType);
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

            return (T)mappedEntity;
        }

        private void GetSets(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var set = this._databaseContext.LooseDataSets
                    .Include(set => set.Attributes)
                    .ToList();

                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(set));
            }
            catch (Exception)
            {
                networkCallbackDelegate.Invoke(null);
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

                var entities = this.GetEntitiesWithOptionalType<LooseDataSet, LooseDataEntity>(types.Item1, types.Item2);
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(entities));
            }
            catch (Exception)
            {
                networkCallbackDelegate.Invoke(null);
            }
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
    }
}