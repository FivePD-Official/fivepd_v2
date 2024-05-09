using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common.Enums;

namespace FivePD.Common.DTO;

#pragma warning disable

public sealed record Vector3ServiceDTOs
{
    public record struct ServiceSpawnData
    {
        public Vector3 location { get; set; }

        public float heading { get; set; }
    }
}