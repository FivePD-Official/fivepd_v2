using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Common.Models;

namespace FivePD.Gamemode.Client.DTO;

#pragma warning disable

internal sealed record InformationUpdateDTO
{
    public readonly string type = "information:update";

    public string Status { get; set; }

    public FDutyStatus DutyStatus { get; set; }

    public FPlayerAvailability Availability { get; set; }

    public ClientCalloutWrapper CalloutWrapper { get; set; }
}