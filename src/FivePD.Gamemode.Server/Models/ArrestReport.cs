// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using FivePD.Gamemode.Server.Models.LooseDataSet;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Models
{
    public class ArrestReport : LooseDataEntity
    {
        public DateTime DateOfArrest { get; set; }

        public string AddressOfArrest { get; set; }

        public string Weapons { get; set; }

        public string Narrative { get; set; }
    }
}