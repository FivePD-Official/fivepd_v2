using FivePD.Common.Enums;
using FivePD.Common.Models;

namespace FivePD.Client.DTO;

#pragma warning disable

internal sealed record InformationUpdateDTO
{
    public readonly string type = "information:update";

    public string Status { get; set; }

    public FDutyStatus DutyStatus { get; set; }

    public FPlayerAvailability Availability { get; set; }

    public ClientCalloutWrapper CalloutWrapper { get; set; }
}