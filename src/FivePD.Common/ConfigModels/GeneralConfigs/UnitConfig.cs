namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record UnitConfig
{
    public int MaxMembers { get; init; } = 0;

    public string Join { get; init; } = "FREE";

    public bool JoinFreely()
    {
        return this.Join.ToUpper() == "FREE";
    }

    public bool JoinByRequest()
    {
        return this.Join.ToUpper() == "REQUEST";
    }
}