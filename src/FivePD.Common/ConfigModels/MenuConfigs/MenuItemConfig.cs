namespace FivePD.Common.ConfigModels.MenuConfigs;

/// <summary>
/// Default config of menu items.
/// </summary>
public sealed record MenuItemConfig
{
    /// <summary>
    /// Gets a value indicating whether the given menu item should be displayed.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether the given menu item should be displayed only for admins.
    /// </summary>
    public bool AdminOnly { get; init; } = false;
}