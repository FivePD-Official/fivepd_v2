using FivePD.Common.Enums;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

internal sealed record PlayerDetailsDTO(string NetworkId, User User, FDutyStatus DutyStatus);