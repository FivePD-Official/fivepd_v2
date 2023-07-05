using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers;
using FivePD.Server.UIModels.Interfaces;

#pragma warning disable S3264
#pragma warning disable CS0067

namespace FivePD.Server.UIModels.MenuModels;

/// <inheritdoc cref="MenuItem" />
public class MenuCheckboxItem : MenuItem, IMenuCheckboxItemWithEvents
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuCheckboxItem"/> class.
    /// </summary>
    /// <param name="title">The title of this item.</param>
    /// <param name="description">The description of this item.</param>
    public MenuCheckboxItem(string title, string description)
        : base(title, description, MenuItemType.Checkbox)
    {
    }

    /// <summary>
    /// Used for firing the item selection event.
    /// </summary>
    /// <param name="player">The player who invoked this event.</param>
    /// <param name="checked">The state of the checkbox.</param>
    public delegate void CheckboxSelect(FPlayer player, bool @checked);

    /// <summary>
    /// Fires when the item has been selected.
    /// </summary>
    public event CheckboxSelect OnSelect;

    /// <summary>
    /// Gets a value indicating whether asd.
    /// </summary>
    public bool Checked { get; private set; }

    /// <inheritdoc/>
    void IMenuCheckboxItemWithEvents.RaiseOnSelect(FPlayer player, bool @checked)
    {
        this.Checked = @checked;
        this.OnSelect?.Invoke(player, @checked);
    }
}