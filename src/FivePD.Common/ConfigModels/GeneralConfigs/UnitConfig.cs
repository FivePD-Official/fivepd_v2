namespace FivePD.Gamemode.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record UnitConfig
{
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