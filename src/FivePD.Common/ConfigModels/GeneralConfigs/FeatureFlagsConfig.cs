namespace FivePD.Gamemode.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record FeatureFlagsConfig
{
    public bool DiscordRichPresence { get; init; }

    public bool DutyNotification { get; init; }

    public bool DisableWantedLevel { get; init; }

    public bool DisableWantedLevelAllTime { get; init; }
}