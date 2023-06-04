using FivePD.Gamemode.Common.ConfigModels.MenuConfigs;

namespace FivePD.Gamemode.Common.ConfigModels;

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