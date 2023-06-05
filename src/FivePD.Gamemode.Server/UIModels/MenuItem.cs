using FivePD.Gamemode.Common.Enums;

#pragma warning disable S3264
#pragma warning disable CS0067

#pragma warning disable S125
#pragma warning disable SA1515

namespace FivePD.Gamemode.Server.UIModels;

/// <summary>
/// Represents a normal menu item in the Nui.
/// </summary>
public abstract class MenuItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuItem"/> class.
    /// </summary>
    /// <param name="title">The title of this item.</param>
    /// <param name="description">The description of this item.</param>
    /// <param name="type">The type of this item.</param>
    protected MenuItem(string title, string description, MenuItemType type)
    {
        this.Title = title;
        this.Enabled = true;
        this.Description = description;
        this.Type = type;
    }

    /// <summary>
    /// Gets the title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets a value indicating whether the item is clickable.
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    /// Gets the type.
    /// </summary>
    public MenuItemType Type { get; }
}