namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record AudioConfig
{
    public AudioFiles Files { get; init; } = new();
}

public sealed record AudioFiles
{
    public string DutyNotification { get; init; }

    public string SingleBeep { get; init; }

    public string DoubleBeep { get; init; }

    public string Notification { get; init; }
}
