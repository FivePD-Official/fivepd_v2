using System.Collections.Generic;

namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public class WarrantsConfig
{
    public int ChanceToHaveAny { get; init; }

    public int MaxAmount { get; init; }

    public List<string> Items { get; init; } = new();
}