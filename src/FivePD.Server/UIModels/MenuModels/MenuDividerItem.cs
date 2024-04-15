using System;
using System.Threading.Tasks;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers;
using FivePD.Server.UIModels.Interfaces;

namespace FivePD.Server.UIModels.MenuModels;

/// <inheritdoc cref="MenuItem" />
public class MenuDividerItem : MenuItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuDividerItem"/> class.
    /// </summary>
    /// <param name="title">The title of this item.</param>
    public MenuDividerItem(string title)
        : base(title, string.Empty, MenuItemType.Divider)
    {
    }
}