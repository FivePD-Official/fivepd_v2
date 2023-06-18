using System.Collections.Generic;

namespace FivePD.Gamemode.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record StatusItemConfig
{
    public string Title { get; init; }

    public string Description { get; init; }

    public List<string> OnSelect { get; init; }
}

internal readonly record struct StatusOnSelectKeys
{
    public const string SetsPlayerCanReceiveCallouts = "SetsPlayerCanReceiveCallouts";

    public const string SetsPlayerCanNotReceiveCallouts = "SetsPlayerCanNotReceiveCallouts";

    public const string CompletesCurrentCallout = "CompletesCurrentCallout";

    public const string TriggersPanicMode = "TriggersPanicMode";
}