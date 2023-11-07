using System.Collections.Generic;
using FivePD.Server.Attributes;
using FivePD.Server.UIModels.MenuModels;

namespace FivePD.Server.EntityWrappers;

/// <summary>
/// Defines the methods which can be used
/// to modify a player's menus.
/// </summary>
[Obfuscate]
internal interface IFPlayerWithMenus
{
    /// <summary>
    /// Sets the given menus for the player.
    /// </summary>
    /// <param name="menus">The new menus.</param>
    void SetMenus(List<Menu> menus);

    /// <summary>
    /// Finds the first menu item with the given hashcode.
    /// </summary>
    /// <param name="hashcode">The menu item's hashcode.</param>
    /// <returns>The found menu item.</returns>
    MenuItem FindMenuItem(string hashcode);
}