// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Dynamic;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.Extensions
{
    /// <summary>
    /// Custom wrapper for the <see cref="JsonConvert"/> class.
    /// </summary>
    public static class Converter
    {
        /// <summary>
        /// Serializes an object by ignoring its reference loops.
        /// Used for EF models.
        /// </summary>
        /// <param name="value">The value to be serialized.</param>
        /// <returns>The object serialized into a string.</returns>
        public static string SerializeObjectWithoutReferenceLoop(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });
        }

        /// <summary>
        /// Serializes, then deserializes an <see cref="ExpandoObject"/> into <typeparamref name="T"/>.
        /// </summary>
        /// <param name="expandoObject">The object to convert.</param>
        /// <typeparam name="T">The type to convert the object.</typeparam>
        /// <returns>The converted object.</returns>
        public static T ConvertTo<T>(this ExpandoObject expandoObject)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(expandoObject));
        }

        /// <summary>
        /// Serializes, then deserializes an <see cref="ExpandoObject"/> into <typeparamref name="T"/>.
        /// </summary>
        /// <param name="expandoObject">The object to convert.</param>
        /// <param name="type">The type to convert.</param>
        /// <typeparam name="T">The type to convert the object.</typeparam>
        /// <returns>The converted object.</returns>
        public static T ConvertTo<T>(this ExpandoObject expandoObject, Type type)
        {
            return (T)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(expandoObject), type);
        }
    }
}