using System.Collections.Generic;

namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record DispatchMenuItemConfig
{
    public string Title { get; init; }

    public string Description { get; init; }

    public List<string> OnSelect { get; init; }
}

internal readonly record struct DispatchActionOnSelectKeys
{
    public const string SetAsStatus = "SET_AS_STATUS";

    public const string SetPlayerOnDuty = "SET_PLAYER_ON_DUTY";

    public const string SetPlayerOffDuty = "SET_PLAYER_OFF_DUTY";

    public const string SetPlayerCanReceiveCallouts = "SET_PLAYER_CAN_RECEIVE_CALLOUTS";

    public const string SetPlayerCanNotReceiveCallouts = "SET_PLAYER_CAN_NOT_RECEIVE_CALLOUTS";

    public const string CompleteCurrentCallout = "COMPLETE_CURRENT_CALLOUT";

    public const string TriggerPanicMode = "TRIGGER_PANIC_MODE";

    public const string RequestBackup = "REQUEST_BACKUP";

    public const string RequestIDCheck = "REQUEST_ID_CHECK";
}