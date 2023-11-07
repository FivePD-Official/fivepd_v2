using FivePD.Common.Enums;
using FivePD.Server.Attributes;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

[Obfuscate]
internal sealed record PlayerDetailsDTO
{
    public string NetworkId { get; init; }

    public User User { get; init; }

    public FDutyStatus DutyStatus { get; init; }

    public string UnitCallsign { get; init; }
}