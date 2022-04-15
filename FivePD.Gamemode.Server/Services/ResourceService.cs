// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.ConfigModels;
using FivePD.Common.Localization;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Validation;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc />
    public class ResourceService : IResourceService
    {
        private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        /// <inheritdoc />
        public Localization GetLocalization(string locale)
        {
            return this.LoadFile<Localization>($"/Localization/{locale}.json", false);
        }

        /// <inheritdoc />
        public List<Item> GetItems()
        {
            return this.LoadFile<List<Item>>("/Config/items.json");
        }

        /// <inheritdoc />
        public List<string> GetFemaleFirstnames()
        {
            return this.LoadStringList("/Config/FemaleFirstnames.txt");
        }

        /// <inheritdoc />
        public List<string> GetMaleFirstnames()
        {
            return this.LoadStringList("/Config/MaleFirstnames.txt");
        }

        /// <inheritdoc />
        public List<string> GetLastnames()
        {
            return this.LoadStringList("/Config/Lastnames.txt");
        }

        /// <summary>
        /// Loads a file's content, splits it by new lines and creates a list from them.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>A string list, containing each line of the file.</returns>
        private List<string> LoadStringList(string path)
        {
            string data = this.InternalLoadFile(path);

            if (this._cache.ContainsKey(path))
            {
                return (List<string>)this._cache[path];
            }

            if (string.IsNullOrEmpty(data))
            {
                return new List<string>();
            }

            var list = data.Split('\n').ToList();
            this._cache[path] = list;

            return list;
        }

        /// <summary>
        /// Loads a file's content, validates it,
        /// If empty, returns T with its default values.
        /// Otherwise returns the data converted to the given type.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="useCache">Determines if the data should be cached.</param>
        /// <returns>The data converted to T.</returns>
        private T LoadFile<T>(string path, bool useCache = true)
        {
            var type = typeof(T);

            if (this._cache.ContainsKey(path) && useCache)
            {
                return (T)this._cache[path];
            }

            string data = this.InternalLoadFile(path);

            if (string.IsNullOrEmpty(data))
            {
                data = "{}";
            }

            var localeData = JsonSchema.FromType(type, new JsonSchemaGeneratorSettings
            {
                DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull,
            });
            var validator = new JsonSchemaValidator();
            var errors = validator.Validate(data, localeData);

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    Debug.WriteLine("Localization error: " + error.Path + ": " + error.Kind);
                }

                // TODO: If there are any errors terminate FivePD ?
                return Activator.CreateInstance<T>();
            }

            var deserializedObject = JsonConvert.DeserializeObject<T>(data);
            if (useCache)
            {
                this._cache[path] = deserializedObject;
            }

            return deserializedObject;
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