using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Gamemode.Common.Enums;

namespace FivePD.Gamemode.Server.API;

#pragma warning disable

public readonly record struct CalloutSetupProperties
{
    public required string Title { get; init; }

    public string Description { get; init; }

    public required FIncidentPriority Priority { get; init; }

    public required float ActivityRadius { get; init; }

    public Vector3 Location { get; init; }

    public IEnumerable<Vector3> Locations { get; init; }
}