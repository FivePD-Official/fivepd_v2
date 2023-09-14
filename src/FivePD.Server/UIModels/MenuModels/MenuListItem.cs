using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers;
using FivePD.Server.UIModels.Interfaces;

namespace FivePD.Server.UIModels.MenuModels;

/// <inheritdoc cref="MenuItem" />
public class MenuListItem : MenuItem, IMenuListItemWithEvents
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuListItem"/> class.
    /// </summary>
    /// <param name="title">The title of this item.</param>
    /// <param name="description">The description of this item.</param>
    /// <param name="items">The items of this list.</param>
    public MenuListItem(string title, string description, List<string> items)
        : base(title, description, MenuItemType.List)
    {
        this.Items = items;
    }

    /// <summary>
    /// Fires when the selected item in the list changes.
    /// </summary>
    public event Func<FPlayer, int, Task> OnIndexChange;

    /// <summary>
    /// Fires when the item has been selected.
    /// </summary>
    public event Func<FPlayer, int, Task> OnSelect;

    /// <summary>
    /// Gets the items.
    /// </summary>
    public List<string> Items { get; }

    /// <summary>
    /// Gets the index.
    /// </summary>
    public int CurrentIndex { get; private set; }

    /// <inheritdoc />
    void IMenuListItemWithEvents.RaiseOnIndexChange(FPlayer player, int index)
    {
        this.CurrentIndex = index;
        this.OnIndexChange?.Invoke(player, index);
    }

    /// <inheritdoc />
    void IMenuListItemWithEvents.RaiseOnSelect(FPlayer player, int index)
    {
        this.OnSelect?.Invoke(player, index);
    }
}