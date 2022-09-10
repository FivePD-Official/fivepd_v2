// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Models.LooseDataSet
{
    public class LooseDataSet
    {
        public LooseDataSet()
        {
            this.Id = this.GetType().ToString();
        }

        public string Id { get; set; }

        public IEnumerable<LooseDataEntity> Entities { get; set; }

        public IEnumerable<LooseDataAttribute> Attributes { get; set; }
    }
}