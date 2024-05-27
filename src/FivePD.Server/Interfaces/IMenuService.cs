using System;
using System.Linq.Expressions;
using FivePD.Server.EntityWrappers;
using FivePD.Server.UIModels.MenuModels;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Handles the UI menus.
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// Registers a menu of the given <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The menu type.</typeparam>
    public void Register<T>()
        where T : Menu;

    /// <summary>
    /// Updated a menu item's property value.
    /// </summary>
    /// <param name="player">The player whose menu item needs to be updated.</param>
    /// <param name="item">The property's menu item.</param>
    /// <param name="property">The property.</param>
    /// <param name="value">The new value.</param>
    /// <typeparam name="TMenu">The type of the menu.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    public void UpdateMenuItem<TMenu, TProperty>(
        FPlayer player,
        Expression<Func<TMenu, MenuItem>> item,
        Expression<Func<MenuItem, TProperty>> property,
        TProperty value)
        where TMenu : Menu;

    /// <summary>
    /// Updated a menu item's property value.
    /// </summary>
    /// <param name="player">The player whose menu item needs to be updated.</param>
    /// <param name="item">The menu item to update.</param>
    /// <param name="property">The property.</param>
    /// <param name="value">The new value.</param>
    /// <typeparam name="TMenu">The type of the menu.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    public void UpdateMenuItem<TMenu, TProperty>(
        FPlayer player,
        MenuItem item,
        Expression<Func<MenuItem, TProperty>> property,
        TProperty value)
        where TMenu : Menu;

    /// <summary>
    /// Updated a menu list item's property value.
    /// </summary>
    /// <param name="player">The player whose menu item needs to be updated.</param>
    /// <param name="item">The property's menu list item.</param>
    /// <param name="property">The property.</param>
    /// <param name="value">The new value.</param>
    /// <typeparam name="TMenu">The type of the menu.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    public void UpdateMenuListItem<TMenu, TProperty>(
        FPlayer player,
        Expression<Func<TMenu, MenuListItem>> item,
        Expression<Func<MenuListItem, TProperty>> property,
        TProperty value)
        where TMenu : Menu;

    /// <summary>
    /// Updated a menu list item's property value.
    /// </summary>
    /// <param name="player">The player whose menu item needs to be updated.</param>
    /// <param name="item">The menu list item to update.</param>
    /// <param name="property">The property.</param>
    /// <param name="value">The new value.</param>
    /// <typeparam name="TMenu">The type of the menu.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    public void UpdateMenuListItem<TMenu, TProperty>(
        FPlayer player,
        MenuListItem item,
        Expression<Func<MenuListItem, TProperty>> property,
        TProperty value)
        where TMenu : Menu;

    /// <summary>
    /// Updates a menu list item's option property value.
    /// </summary>
    /// <param name="player">A.</param>
    /// <param name="item">B.</param>
    /// <param name="index">C.</param>
    /// <param name="property">D.</param>
    /// <param name="value">AA.</param>
    /// <typeparam name="TMenu">ASD.</typeparam>
    /// <typeparam name="TProperty">TProperty.</typeparam>
    public void UpdateMenuListOption<TMenu, TProperty>(
        FPlayer player,
        Expression<Func<TMenu, MenuListItem>> item,
        int index,
        Expression<Func<MenuListOption, TProperty>> property,
        TProperty value)
        where TMenu : Menu;

    /// <summary>
    /// Adds a menu item to the menu of the given <typeparamref name="TMenu"/>.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="item">The item to add.</param>
    /// <typeparam name="TMenu">The type of the menu.</typeparam>
    /// <returns>The ID of the item.</returns>
    public string AddMenuItem<TMenu>(FPlayer player, MenuItem item)
        where TMenu : Menu;

    /// <summary>
    /// Removes a menu item from the menu of the given <typeparamref name="TMenu"/>.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="item">The ID of the item to remove.</param>
    /// <typeparam name="TMenu">The type of the menu.</typeparam>
    public void RemoveMenuItem<TMenu>(FPlayer player, MenuItem item)
        where TMenu : Menu;
}

#pragma warning disable
internal interface IInternalMenuService
{
    public void RegisterMenus();
}
#pragma warning restore