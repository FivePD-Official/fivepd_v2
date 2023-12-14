using System.Collections.Generic;

namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record PedsConfig
{
    public int ChanceToReuseData { get; init; }

    public PedLicensesConfig Licenses { get; init; } = new();

    public WarrantsConfig Warrants { get; init; } = new();

    public AlcoholConfig Alcohol { get; init; } = new();

    public List<DrugConfig> Drugs { get; init; } = new();
}