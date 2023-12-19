namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record VehicleDocumentsConfig
{
    public VehicleDocumentConfig Insurance { get; init; } = new();

    public VehicleDocumentConfig Registration { get; init; } = new();

    public sealed record VehicleDocumentConfig
    {
        public int ChanceToHave { get; init; }

        public int ChanceForExpired { get; init; }
    }
}