using System;
using System.Threading.Tasks;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers;
using FivePD.Server.UIModels.Interfaces;

namespace FivePD.Server.UIModels.MenuModels;

/// <inheritdoc cref="MenuItem" />
public class MenuButtonItem : MenuItem, IMenuButtonItemWithEvents
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuButtonItem"/> class.
    /// </summary>
    /// <param name="title">The title of this item.</param>
    /// <param name="description">The description of this item.</param>
    /// <param name="enabled">The state of this item.</param>
    public MenuButtonItem(string title, string description, bool enabled = true)
        : base(title, description, enabled, MenuItemType.Button)
    {
    }

    /// <summary>
    /// Fires when the item has been selected.
    /// </summary>
    public event Func<FPlayer, Task> OnSelect;

    /// <inheritdoc/>
    void IMenuButtonItemWithEvents.RaiseOnSelect(FPlayer player)
    {
        this.OnSelect?.Invoke(player);
    }
}