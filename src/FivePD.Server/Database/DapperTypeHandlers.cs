using System;
using System.Collections.Generic;
using System.Data;
using CitizenFX.Core;
using Dapper;
using FivePD.Server.Models;
using Newtonsoft.Json;

namespace FivePD.Server.Database;

/// <summary>
/// Custom Dapper type handlers for complex types.
/// </summary>
public static class DapperTypeHandlers
{
    /// <summary>
    /// Registers all custom type handlers with Dapper.
    /// </summary>
    public static void Register()
    {
        SqlMapper.AddTypeHandler(new CallsignFormatTypeHandler());
        SqlMapper.AddTypeHandler(new ListStringTypeHandler());
        SqlMapper.AddTypeHandler(new ListIntTypeHandler());
        SqlMapper.AddTypeHandler(new Vector3TypeHandler());
        SqlMapper.AddTypeHandler(new RankPermissionsTypeHandler());
        SqlMapper.AddTypeHandler(new ListVector3TypeHandler());
    }

    /// <summary>
    /// Type handler for processing and mapping List of Vector3 objects in database operations using Dapper.
    /// </summary>
    private sealed class ListVector3TypeHandler : SqlMapper.TypeHandler<List<Vector3>>
    {
        public override List<Vector3> Parse(object value)
        {
            if (value == DBNull.Value)
            {
                return new List<Vector3>();
            }

            var json = value.ToString();
            if (string.IsNullOrEmpty(json))
            {
                return new List<Vector3>();
            }

            return JsonConvert.DeserializeObject<List<Vector3>>(
                json,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public override void SetValue(IDbDataParameter parameter, List<Vector3> value)
        {
            parameter.Value = value is null
                ? DBNull.Value
                : JsonConvert.SerializeObject(
                    value,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }

    /// <summary>
    /// Type handler for processing and mapping List of CallsignFormatItem objects in database operations using Dapper.
    /// </summary>
    private sealed class CallsignFormatTypeHandler : SqlMapper.TypeHandler<List<CallsignFormatItem>>
    {
        public override List<CallsignFormatItem> Parse(object value)
        {
            if (value == DBNull.Value)
            {
                return new List<CallsignFormatItem>();
            }

            var json = value.ToString();
            if (string.IsNullOrEmpty(json))
            {
                return new List<CallsignFormatItem>();
            }

            return JsonConvert.DeserializeObject<List<CallsignFormatItem>>(
                json,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public override void SetValue(IDbDataParameter parameter, List<CallsignFormatItem> value)
        {
            parameter.Value = value is null
                ? DBNull.Value
                : JsonConvert.SerializeObject(
                    value,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }

    /// <summary>
    /// Type handler for processing and mapping List of string objects in database operations using Dapper.
    /// </summary>
    private sealed class ListStringTypeHandler : SqlMapper.TypeHandler<List<string>>
    {
        public override List<string> Parse(object value)
        {
            if (value == DBNull.Value)
            {
                return new List<string>();
            }

            var json = value.ToString();
            if (string.IsNullOrEmpty(json))
            {
                return new List<string>();
            }

            return JsonConvert.DeserializeObject<List<string>>(
                json,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public override void SetValue(IDbDataParameter parameter, List<string> value)
        {
            parameter.Value = value is null
                ? DBNull.Value
                : JsonConvert.SerializeObject(
                    value,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }

    /// <summary>
    /// Type handler for processing and mapping List of int objects in database operations using Dapper.
    /// </summary>
    private sealed class ListIntTypeHandler : SqlMapper.TypeHandler<List<int>>
    {
        public override List<int> Parse(object value)
        {
            if (value == DBNull.Value)
            {
                return new List<int>();
            }

            var json = value.ToString();
            if (string.IsNullOrEmpty(json))
            {
                return new List<int>();
            }

            return JsonConvert.DeserializeObject<List<int>>(
                json,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public override void SetValue(IDbDataParameter parameter, List<int> value)
        {
            parameter.Value = value is null
                ? DBNull.Value
                : JsonConvert.SerializeObject(
                    value,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }

    /// <summary>
    /// Type handler for processing and mapping Vector3 objects in database operations using Dapper.
    /// </summary>
    private sealed class Vector3TypeHandler : SqlMapper.TypeHandler<Vector3>
    {
        public override Vector3 Parse(object value)
        {
            if (value == DBNull.Value)
            {
                return default;
            }

            var json = value.ToString();
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<Vector3>(
                json,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public override void SetValue(IDbDataParameter parameter, Vector3 value)
        {
            parameter.Value = JsonConvert.SerializeObject(
                value,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }

    /// <summary>
    /// Custom Dapper type handler for serializing and deserializing <see cref="Rank.Permissions"/> objects to and from JSON format.
    /// </summary>
    private sealed class RankPermissionsTypeHandler : SqlMapper.TypeHandler<Rank.Permissions>
    {
        public override Rank.Permissions Parse(object value)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            var json = value.ToString();
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Rank.Permissions>(
                json,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public override void SetValue(IDbDataParameter parameter, Rank.Permissions value)
        {
            parameter.Value = value is null
                ? DBNull.Value
                : JsonConvert.SerializeObject(
                    value,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}