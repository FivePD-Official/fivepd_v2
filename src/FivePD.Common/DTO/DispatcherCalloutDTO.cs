using System;
using CitizenFX.Core;
using FivePD.Common.Enums;

namespace FivePD.Common.DTO;

#pragma warning disable

internal sealed record DispatcherCalloutDTO
{
    public string Id { get; init; }

    public string Title { get; init; }

    public string Description { get; init; }

    public FIncidentPriority Priority { get; init; }

    public Vector3 Location { get; init; }

    public DateTime GeneratedAt { get; init; }
}

internal sealed record DispatcherCalloutDTOClientWrapper(DispatcherCalloutDTO Callout, string Address)
{
    public string type = Events.Dispatch.SendCalloutToDispatcher;
}