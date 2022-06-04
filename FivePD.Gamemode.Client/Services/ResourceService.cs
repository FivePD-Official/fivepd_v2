// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Concurrent;
using CitizenFX.Core;
using FivePD.Gamemode.Client.Interfaces;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class ResourceService : IResourceService
    {
        private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        /// <inheritdoc />
        public T LoadFile<T>(string path, bool useCache = true)
        {
            if (this._cache.ContainsKey(path) && useCache)
            {
                return (T)this._cache[path];
            }

            string data = this.InternalLoadFile(path);

            if (string.IsNullOrEmpty(data))
            {
                data = "{}";
            }

            try
            {
                var deserializedObject = JsonConvert.DeserializeObject<T>(data);
                if (useCache)
                {
                    this._cache[path] = deserializedObject;
                }

                return deserializedObject;
            }
            catch (Exception)
            {
                Debug.WriteLine($"Error parsing the following file: {path}. Make sure that the JSON formatting is correct.");
            }

            return Activator.CreateInstance<T>();
        }

        /// <summary>
        /// Loads a file's content then returns it without validation.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The content of the file.</returns>
        private string InternalLoadFile(string path)
        {
            return Cfx.API.LoadResourceFile(Cfx.API.GetCurrentResourceName(),  path);
        }
    }
}