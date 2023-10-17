using CitizenFX.Core;
using FivePD.Common.Enums;

namespace FivePD.Common.Models;

#pragma warning disable

internal sealed record ClientIncidentDTO
{
    public string Id { get; init; }
    
    public FIncidentType Type { get; init; }

    public string Title { get; init; }

    public string Description { get; init; }

    public FIncidentPriority Priority { get; init; }

    public float ActivityRadius { get; init; }

    public Vector3 Location { get; init; }
    
    public int TrafficStopInvolvedVehicle { get; init; }
}

internal sealed record ClientIncidentWithAddressDTO(ClientIncidentDTO Incident, string Address);