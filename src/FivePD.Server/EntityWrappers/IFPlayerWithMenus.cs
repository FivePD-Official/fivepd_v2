using System.Collections.Generic;
using FivePD.Server.Attributes;
using FivePD.Server.UIModels.MenuModels;

namespace FivePD.Server.EntityWrappers;

/// <summary>
/// Defines the methods which can be used to modify a player's menus.
/// This interface is used internally by the menu system to manage player menu state.
/// </summary>
[Obfuscate]
internal interface IFPlayerWithMenus
{
    /// <summary>
    /// Sets the given menus for the player, replacing any existing menus.
    /// </summary>
    /// <param name="menus">The new collection of menus to assign to the player.</param>
    void SetMenus(List<Menu> menus);

    /// <summary>
    /// Finds the first menu with the given id.
    /// </summary>
    /// <param name="id">The unique identifier of the menu to find.</param>
    /// <returns>The found menu, or null if no menu matches the given id.</returns>
    Menu FindMenu(string id);

    /// <summary>
    /// Finds the first menu item with the given hashcode.
    /// </summary>
    /// <param name="hashcode">The hashcode that uniquely identifies the menu item.</param>
    /// <returns>The found menu item, or null if no menu item matches the given hashcode.</returns>
    MenuItem FindMenuItem(string hashcode);
}