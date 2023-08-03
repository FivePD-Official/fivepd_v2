using System.Collections.Generic;

namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record DispatchActionItemConfig
{
    public string Title { get; init; }

    public string Description { get; init; }

    public List<string> OnSelect { get; init; }
}

internal readonly record struct DispatchActionOnSelectKeys
{
    public const string SetStatus = "SetStatus";

    public const string SetPlayerOnDuty = "SetPlayerOnDuty";

    public const string SetPlayerOffDuty = "SetPlayerOffDuty";

    public const string SetPlayerCanReceiveCallouts = "SetPlayerCanReceiveCallouts";

    public const string SetPlayerCanNotReceiveCallouts = "SetPlayerCanNotReceiveCallouts";

    public const string CompleteCurrentCallout = "CompleteCurrentCallout";

    public const string TriggerPanicMode = "TriggerPanicMode";

    public const string RequestBackup = "RequestBackup";
}