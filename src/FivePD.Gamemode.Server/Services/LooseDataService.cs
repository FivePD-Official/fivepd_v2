// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Gamemode.Server.Database;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models.LooseDataSet;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

#pragma warning disable CS1591
#pragma warning disable SA1600

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

                var properties = entityToAdd.GetType().GetProperties();
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
        
        public T GetEntity<T>(int id)
        {
            try
            {
                var entity = this._databaseContext.LooseDataEntities
                    .Include(entity => entity.Values)
                    .Include(entity => entity.Set)
                    .ThenInclude(set => set.Attributes)
                    .FirstOrDefault(entity => entity.Id == id);
                if (entity is null)
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

                var mappedEntity = Activator.CreateInstance<T>();
                foreach (var item in attributeValueMap)
                {
                    var propertyInfo = mappedEntity.GetType().GetProperty(item.Key);
                    propertyInfo?.SetValue(mappedEntity, Convert.ChangeType(item.Value, propertyInfo.PropertyType), null);
                }

                return mappedEntity;
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        public void DeleteEntity<TS, TE>(int id)
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
    }
}