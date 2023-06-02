using System;
using System.Linq.Expressions;
using FivePD.Common.Models;
using FivePD.Common.UIModels;

namespace FivePD.Gamemode.Server.Interfaces;

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
    /// <typeparam name="T">The type of the menu.</typeparam>
    /// <example>
    /// <code>
    /// this._menuService.UpdateMenuItem&lt;DutyMenu&gt;(player, menu => menu.DutyCheckboxItem, item => item.Title, "New title");
    /// </code>
    /// </example>
    public void UpdateMenuItem<T>(FPlayer player, Expression<Func<T, MenuItem>> item, Expression<Func<MenuItem, object>> property, object value)
        where T : Menu;
}