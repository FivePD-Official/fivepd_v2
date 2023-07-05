namespace FivePD.Common.ConfigModels.MenuConfigs;

/// <summary>
/// The config of the duty menu.
/// </summary>
public sealed record DutyMenuConfig
{
    /// <summary>
    /// Gets the config of the duty status toggle item.
    /// </summary>
    public MenuItemConfig DutyToggle { get; init; } = default;

    /// <summary>
    /// Gets the config of the vehicle spawn item.
    /// </summary>
    public MenuItemConfig SpawnVehicle { get; init; } = default;

    /// <summary>
    /// Gets the config of the loadout spawner item.
    /// </summary>
    public MenuItemConfig GetLoadout { get; init; } = default;
}