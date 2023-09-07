namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record PedsConfig
{
    public int ChanceToReusePedHistory { get; init; }

    public LicensesConfig Licenses { get; init; } = new();

    public WarrantsConfig Warrants { get; init; } = new();
}