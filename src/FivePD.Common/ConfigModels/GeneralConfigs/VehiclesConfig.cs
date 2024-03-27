namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record VehiclesConfig
{
    public VehicleDocumentsConfig Documents { get; init; } = new();
}