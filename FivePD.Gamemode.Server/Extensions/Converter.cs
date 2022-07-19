// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

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
    }
}