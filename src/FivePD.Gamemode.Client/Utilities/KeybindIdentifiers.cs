namespace FivePD.Gamemode.Client.Utilities;

/// <summary>
/// Contains the identifiers of all FivePD keybindings.
/// </summary>
internal readonly record struct KeybindIdentifiers
{
    /// <summary>
    /// Closes any opened menu.
    /// </summary>
    public const string MenuClose = "fivepd:menuClose";

    /// <summary>
    /// Moves up by one item in menus.
    /// </summary>
    public const string MenuMoveUp = "fivepd:menuMoveUp";

    /// <summary>
    /// Moves down by one item in menus.
    /// </summary>
    public const string MenuMoveDown = "fivepd:menuMoveDown";

    /// <summary>
    /// Moves left by one item in menus.
    /// </summary>
    public const string MenuMoveLeft = "fivepd:menuMoveLeft";

    /// <summary>
    /// Moves right by one item in menus.
    /// </summary>
    public const string MenuMoveRight = "fivepd:menuMoveRight";

    /// <summary>
    /// Invokes the action of the currently selected menu item.
    /// </summary>
    public const string MenuItemSelect = "fivepd:menuItemSelect";
}