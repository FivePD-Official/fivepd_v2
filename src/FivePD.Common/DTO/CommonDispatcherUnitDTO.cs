using System.Collections.Generic;
using FivePD.Common.Enums;

namespace FivePD.Common.DTO;

#pragma warning disable

internal record struct CommonDispatcherUnitDTO
{
    public string Callsign;

    public IEnumerable<SimplePlayerDTO> Players;

    public string Status;

    public FUnitAvailability Availability;
}

internal sealed record CommonDispatcherUnitDTOClientWrapper(CommonDispatcherUnitDTO Unit)
{
    public string type = Common.Events.Dispatch.OnUnitCreated;
}