using FivePD.Common.Enums;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

internal sealed record PlayerDetailsDTO
{
    public string NetworkId { get; init; }

    public User User { get; init; }

    public FDutyStatus DutyStatus { get; init; }

    public string UnitCallsign { get; init; }
}