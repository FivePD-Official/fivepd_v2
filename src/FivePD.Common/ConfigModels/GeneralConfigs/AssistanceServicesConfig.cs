using System.Collections.Generic;
using CitizenFX.Core;

namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record AssistanceServicesConfig
{
    public string SpawnType { get; init; } = "AUTO";

    public bool SpawnTypeAuto()
    {
        return this.SpawnType.ToUpper() == "AUTO";
    }

    public bool SpawnTypeManual()
    {
        return this.SpawnType.ToUpper() == "MANUAL";
    }

    public AssistanceServiceSpawnLocations SpawnLocations { get; init; } = new();
}

public sealed record AssistanceServiceSpawnLocations
{
    public List<Vector3> Ambulance { get; init; } = new();

    public List<Vector3> TowTruck { get; init; } = new();
}