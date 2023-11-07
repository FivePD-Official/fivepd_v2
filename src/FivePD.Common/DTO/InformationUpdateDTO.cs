using FivePD.Common.Enums;
using FivePD.Common.Models;

namespace FivePD.Common.DTO;

#pragma warning disable

internal sealed record InformationUpdateDTO
{
    public readonly string Event = "fivepd:ui:information:update";

    public string Status { get; set; }

    public FDutyStatus DutyStatus { get; set; }

    public FUnitAvailability Availability { get; set; }

    public ClientIncidentWithAddressDTO IncidentWrapper { get; set; }
}