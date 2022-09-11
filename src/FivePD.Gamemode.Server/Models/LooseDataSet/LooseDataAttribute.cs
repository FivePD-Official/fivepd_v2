// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

#pragma warning disable CS1591
#pragma warning disable SA1600
#pragma warning disable SA1602

namespace FivePD.Gamemode.Server.Models.LooseDataSet
{
    public class LooseDataAttribute
    {
        public enum LooseDataType
        {
            String,
            Integer,
            Bool,
            DateTime,
        }

        public int Id { get; set; }

        public LooseDataSet Set { get; set; }

        public LooseDataType DataType { get; set; }

        public string Name { get; set; }

        public bool Required { get; set; }

        public string Placeholder { get; set; }

        public string Label { get; set; }

        public string HelpLabel { get; set; }

        public AttributeOptions Options { get; set; }

        public class AttributeOptions
        {
            public Dictionary<int, string> Items { get; set; }
        }
    }
}