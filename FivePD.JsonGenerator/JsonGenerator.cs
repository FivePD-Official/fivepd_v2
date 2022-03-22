// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.IO;
using FivePD.Common.Localization;
using Newtonsoft.Json;

namespace FivePD.JsonGenerator
{
    /// <summary>
    /// Generates JSON files from the given types.
    /// </summary>
    public static class JsonGenerator
    {
        /// <summary>
        /// Generates JSON files from the given types..
        /// </summary>
        public static void Main()
        {
            Write(typeof(Localization), "Localization/en.json");
        }

        private static void Write(Type typeOfObject, string path)
        {
            var data = JsonConvert.SerializeObject(Activator.CreateInstance(typeOfObject), Formatting.Indented);
            File.WriteAllText($"../FivePD.Gamemode.Statics/{path}", data);
        }
    }
}