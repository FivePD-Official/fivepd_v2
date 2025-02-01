namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record AudioConfig
{
    public AudioFiles Files { get; init; } = new();
}

public sealed record AudioFiles
{
    public string DutyNotification { get; init; }

    public string UnitJoinRequest { get; init; }

    public string BackupRequested { get; init; }
    
    public string CalloutDispatched { get; init; }
}
