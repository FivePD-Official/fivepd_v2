using FivePD.Common.ConfigModels.MenuConfigs;

namespace FivePD.Common.ConfigModels;

/// <summary>
/// Contains the config of all official menus.
/// </summary>
public sealed record MenuConfig
{
    /// <summary>
    /// Gets the duty menu config.
    /// </summary>
    public required DutyMenuConfig DutyMenu { get; init; } = new();
}