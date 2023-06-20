using FivePD.Gamemode.Common.Enums;

namespace FivePD.Gamemode.Client.DTO;

#pragma warning disable

public sealed record InformationUpdateDTO
{
    public readonly string type = "information:update";

    public string Status { get; set; }

    public FDutyStatus DutyStatus { get; set; }

    public FPlayerAvailability Availability { get; set; }
}