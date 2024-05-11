using FivePD.Common.Models;
using FivePD.Server.Attributes;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

[Obfuscate]
internal record struct SpeedZoneWithPlayerDTO
{
    public string Id;

    public SpeedZone SpeedZone;

    public string PlayerName;
}