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
}