// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Models.LooseDataSet
{
    public class LooseDataValue
    {
        public int Id { get; set; }

        public LooseDataAttribute Attribute { get; set; }

        public LooseDataEntity Entity { get; set; }

        public string Value { get; set; }
    }
}