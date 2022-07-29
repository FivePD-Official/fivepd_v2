// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using Newtonsoft.Json;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.NuiModels
{
    public class CreateDepartmentFields
    {
        [JsonProperty]
        public string Name { get; private set; }

        [JsonProperty]
        public string Callsign { get; private set; } = string.Empty;

        [JsonProperty]
        public string CallsignFormat { get; private set; }

        [JsonProperty]
        public int MaxUsers { get; private set; }
    }
}