namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record FeatureFlagsConfig
{
    public bool AutoEnableFivePDAccess { get; init; } = true;
    public bool DiscordRichPresence { get; init; }
    public bool DutyNotification { get; init; }
    public bool DisableWantedLevel { get; init; }
    public bool DisableWantedLevelAllTime { get; init; }
    public bool Backups { get; init; }
    public bool ProgressionSystem { get; init; }
    public bool Messages { get; init; }
}