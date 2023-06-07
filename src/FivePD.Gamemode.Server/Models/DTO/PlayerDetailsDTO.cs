using FivePD.Gamemode.Common.Enums;

namespace FivePD.Gamemode.Server.Models.DTO;

#pragma warning disable

internal sealed record PlayerDetailsDTO(string NetworkId, User User, FDutyStatus DutyStatus);