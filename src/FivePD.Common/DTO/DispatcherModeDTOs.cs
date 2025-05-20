using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.DTO.Dispatch;
using FivePD.Common.Enums;

namespace FivePD.Client.DTO;

#pragma warning disable

internal sealed record DispatcherModeDTOs
{
    internal sealed record Callout
    {
        public string Id { get; init; }

        public string Title { get; init; }

        public string Description { get; init; }

        public FPriority Priority { get; init; }

        public Vector3 Location { get; init; }

        public DateTime GeneratedAt { get; init; }
        
        public List<string> Units { get; init; }
    }

    internal sealed record PostCalloutClient(Callout Callout, string Address) : RecordWithEventName(Events.Dispatch.SendCalloutToDispatcher);

    internal sealed record RemoveCalloutClient(string CalloutId) : RecordWithEventName(Events.Dispatch.RemoveCalloutFromDispatcher);
    
    internal sealed record Unit() : RecordWithEventName(Events.Dispatch.OnUnitCreated)
    {
        public string Callsign;

        public IEnumerable<SimplePlayerDTO> Players;

        public string Status;

        public FUnitAvailability Availability;
    }

    internal sealed record RemoveUnitClient(string UnitCallsign) : RecordWithEventName(Events.Dispatch.OnUnitRemoved);

    internal sealed record UpdateRequest() : RecordWithEventName(Events.Dispatch.UpdateRequest);
}