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
    
    public AmbulanceConfig Ambulance { get; init; } = new();
    public AmbulanceConfig AirAmbulance { get; init; } = new();
    public BaseServiceConfig AnimalControl { get; init; } = new();
    public BaseServiceConfig Coroner { get; init; } = new();
    public BaseServiceConfig Mechanic { get; init; } = new();
    public BaseServiceConfig PrisonTransport { get; init; } = new();
    public BaseServiceConfig RideShare { get; init; } = new();
    public BaseServiceConfig TowTruck { get; init; } = new();
}


public record BaseServiceConfig
{
    public bool Enabled { get; init; }
    public List<Vector3> SpawnLocations { get; init; } = new();
}

public sealed record AmbulanceConfig : BaseServiceConfig
{
    public int ChanceToRevive { get; init; }
}