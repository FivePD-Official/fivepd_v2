namespace FivePD.Common.ConfigModels;

#pragma warning disable

public sealed record DrugConfig
{
    public string Name { get; init; }

    public int ChanceToHave { get; init; }
}