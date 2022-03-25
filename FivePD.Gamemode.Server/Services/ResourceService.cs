// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using CitizenFX.Core;
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
        /// <inheritdoc />
        public T LoadFile<T>(string path)
        {
            string data = this.InternalLoadFile(path);

            if (string.IsNullOrEmpty(data))
            {
                data = "{}";
            }

            var localeData = JsonSchema.FromType(typeof(T), new JsonSchemaGeneratorSettings
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

            return JsonConvert.DeserializeObject<T>(data);
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