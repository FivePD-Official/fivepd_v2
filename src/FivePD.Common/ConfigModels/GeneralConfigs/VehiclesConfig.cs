namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record VehiclesConfig
{
    public VehicleDocumentsConfig Documents { get; init; } = new();

    // public PoliceConfig Police { get; init; } = new();

    // public sealed record PoliceConfig
    // {
    //     public bool EnableSpikeStripInAllVehicles { get; init; }
    // }
}