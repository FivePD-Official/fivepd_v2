namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record CalloutConfig
{
    public bool AutomaticGeneration { get; init; }

    public int MinIntervalInMs { get; init; }

    public int MaxIntervalInMs { get; init; }

    public int MaxActiveCallouts { get; init; }
}