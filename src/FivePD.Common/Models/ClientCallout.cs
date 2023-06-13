using CitizenFX.Core;
using FivePD.Gamemode.Common.Enums;

namespace FivePD.Gamemode.Common.Models;

#pragma warning disable

internal sealed record ClientCallout
{
    public string Id { get; init; }

    public string Title { get; init; }

    public FIncidentPriority Priority { get; init; }

    public float ActivityRadius { get; init; }

    public Vector3 Location { get; init; }
}