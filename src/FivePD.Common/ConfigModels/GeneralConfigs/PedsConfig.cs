using System.Collections.Generic;

namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record PedsConfig
{
    public int ChanceToReusePedHistory { get; init; }

    public int ChanceForAnyWarrant { get; init; }

    public int MaxWarrants { get; init; }

    public List<string> Warrants { get; init; } = new();
}