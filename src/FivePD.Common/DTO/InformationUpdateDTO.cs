using FivePD.Common.Attributes;
using FivePD.Common.Enums;
using FivePD.Common.Models;

namespace FivePD.Common.DTO;

#pragma warning disable

[Obfuscate]
internal sealed record InformationUpdateDTO
{
    public readonly string Event = Events.UI.Information.Update;

    public string Status { get; set; }

    public bool PanicMode { get; set; }

    public FDutyStatus DutyStatus { get; set; }

    public FUnitAvailability Availability { get; set; }

    public ClientIncidentWithAddressDTO IncidentWrapper { get; set; }
}