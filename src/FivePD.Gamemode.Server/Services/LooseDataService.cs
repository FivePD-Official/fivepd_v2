// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Linq;
using FivePD.Gamemode.Server.Database;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models.LooseDataSet;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

#pragma warning disable CS1591
#pragma warning disable SA1600
#pragma warning disable S125
#pragma warning disable SA1512
#pragma warning disable SA1515

namespace FivePD.Gamemode.Server.Services
{
    public class LooseDataService : ILooseDataService
    {
        private readonly DatabaseContext _databaseContext;

        public LooseDataService(DatabaseContext databaseContext)
        {
            this._databaseContext = databaseContext;
        }

        public void RegisterSetWithAttributes<TS>(List<LooseDataAttribute> attributes)
            where TS : LooseDataSet
        {
            try
            {
                var type = typeof(TS);

                var foundSet = this._databaseContext.LooseDataSets
                    .Include(s => s.Attributes)
                    .FirstOrDefault(s => s.Id == type.ToString());
                if (foundSet is null)
                {
                    foundSet = Activator.CreateInstance<TS>();
                    this._databaseContext.LooseDataSets.Add(foundSet);
                }

                if (foundSet.Attributes != null && foundSet.Attributes.Any())
                {
                    return;
                }

                foundSet.Attributes = attributes;
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
                var type = typeof(TS);

                var foundSet = this._databaseContext.LooseDataSets
                    .Include(s => s.Attributes)
                    .FirstOrDefault(s => s.Id == type.ToString());
                if (foundSet is null)
                {
                    return;
                }

                var attributeValueMap = new Dictionary<LooseDataAttribute, object>();
                foreach (var attr in foundSet.Attributes)
                {
                    attributeValueMap[attr] = null;
                }

                var properties = type.GetProperties();
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
                var values = new List<LooseDataValue>();
                foreach (var item in attributeValueMap)
                {
                    var entityValue = new LooseDataValue
                    {
                        Attribute = item.Key,
                        Entity = entity,
                        Value = JsonConvert.SerializeObject(item.Value),
                    };
                    values.Add(entityValue);
                }

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
            try
            {
                var entity = this._databaseContext.LooseDataEntities
                    .Include(entity => entity.Values)
                    .ThenInclude(values => values.Attribute)
                    .Include(entity => entity.Set)
                    .ThenInclude(set => set.Attributes)
                    .FirstOrDefault(entity => entity.Id == id);

                return entity is null ? default : this.MapEntity<T>(entity);
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        public List<TE> GetEntities<TS, TE>()
            where TS : LooseDataSet
            where TE : LooseDataEntity
        {
            try
            {
                var entities = this._databaseContext.LooseDataEntities
                    .Include(entity => entity.Values)
                    .Include(entity => entity.Set)
                    .ThenInclude(set => set.Attributes)
                    .Where(entity => entity.Set.Id == typeof(TS).ToString())
                    .ToList();

                return entities.Select(this.MapEntity<TE>).ToList();
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        public void DeleteEntity<TS>(int id)
            where TS : LooseDataSet
        {
            try
            {
                var type = typeof(TS);

                var set = this._databaseContext.LooseDataSets
                    .Include(set => set.Attributes)
                    .FirstOrDefault(set => set.Id == type.ToString());
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

        private T MapEntity<T>(LooseDataEntity entity)
            where T : LooseDataEntity
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

            foreach (var value in entity.Values)
            {
                attributeValueMap[value.Attribute.Name] = JsonConvert.DeserializeObject(value.Value);
            }

            attributeValueMap["Id"] = entity.Id;
            attributeValueMap["SetId"] = entity.SetId;

            var mappedEntity = Activator.CreateInstance<T>();
            foreach (var item in attributeValueMap)
            {
                if (item.Value is null)
                {
                    continue;
                }

                var propertyInfo = mappedEntity.GetType().GetProperty(item.Key);
                propertyInfo?.SetValue(mappedEntity, Convert.ChangeType(item.Value, propertyInfo.PropertyType), null);
            }

            return mappedEntity;
        }
    }
}