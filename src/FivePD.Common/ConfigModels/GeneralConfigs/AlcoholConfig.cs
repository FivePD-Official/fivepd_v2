namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record AlcoholConfig
{
    public int ChanceToHave { get; init; }

    public int ChanceForOverLegalLimit { get; init; }

    public double LegalLimit { get; init; }
}