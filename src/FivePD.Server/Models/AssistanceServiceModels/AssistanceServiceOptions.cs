using System.Collections.Generic;
using CitizenFX.Core;

namespace FivePD.Server.Models.AssistanceServiceModels;

#pragma warning disable

public sealed record AssistanceServiceOptions
{
    public string RequestLabel { get; init; }

    public string CancelLabel { get; init; }

    public string Description { get; init; }

    // public List<Vector3> SpawnLocations { get; init; } = new();

    public float SpawnHeading { get; init; }
};