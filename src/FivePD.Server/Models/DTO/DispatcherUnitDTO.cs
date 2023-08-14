using System.Collections.Generic;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Server.API;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

internal record struct DispatcherUnitDTO
{
    public string Callsign;

    public IEnumerable<SimplePlayerDTO> Players;

    public string Status;

    public FUnitAvailability Availability;

    public SimpleCallout AttachedCallout;
}