using System;
using FivePD.Common.Models;

namespace FivePD.Common.DTO;

#pragma warning disable

internal sealed record CreateSpeedZoneDTO
{
    public Guid Id { get; set; }
    
    public SpeedZone SpeedZone { get; set; }
}