using System;
using System.Collections.Generic;
using System.Data;
using CitizenFX.Core;
using Dapper;
using FivePD.Common.ConfigModels;
using FivePD.Server.Models;
using FivePD.Server.Models.VariableData;
using Newtonsoft.Json;

namespace FivePD.Server.Database;

#pragma warning disable

/// <summary>
/// Custom Dapper type handlers for complex types.
/// </summary>
public static class DapperTypeHandlers
{
    public static void Register()
    {
        SqlMapper.AddTypeHandler(new JsonTypeHandler<List<CallsignFormatItem>>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<List<string>>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<List<int>>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<List<Vector3>>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<List<Item>>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<List<Drug>>());

        SqlMapper.AddTypeHandler(new JsonTypeHandler<Vector3>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<Rank.Permissions>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<VariableDataAttribute.AttributeOptions>());
    }

    private class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
    {
        private readonly JsonSerializerSettings _settings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
        };

        public override T Parse(object value)
        {
            if (value == DBNull.Value)
            {
                return default;
            }

            var json = value.ToString();
            return string.IsNullOrEmpty(json)
                ? default
                : JsonConvert.DeserializeObject<T>(json, this._settings);
        }

        public override void SetValue(IDbDataParameter parameter, T value)
        {
            parameter.Value = value == null
                ? DBNull.Value
                : JsonConvert.SerializeObject(value, this._settings);
        }
    }
}