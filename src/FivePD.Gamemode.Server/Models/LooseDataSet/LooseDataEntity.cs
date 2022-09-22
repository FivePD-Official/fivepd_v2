// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Models.LooseDataSet
{
    public class LooseDataEntity
    {
        public int Id { get; set; }

        public string SetId { get; set; }

        public LooseDataSet Set { get; set; }

        public IList<LooseDataValue> Values { get; set; }

        [NotMapped]
        public Dictionary<string, object> MappedValues { get; set; }
    }
}