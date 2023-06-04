using System.Collections.Generic;

namespace FivePD.Gamemode.Common.UIModels;

/// <summary>
/// Defines a UI menu.
/// </summary>
public abstract class Menu
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Menu"/> class.
    /// </summary>
    /// <param name="id">The id of the menu. We recommend using UUID to prevent any conflict.</param>
    /// <param name="title">The title of the menu.</param>
    /// <param name="key">The keybind of this menu.</param>
    /// <param name="useLocalization">Determines whether this menu's Title should be used as a key for localizations.</param>
    protected Menu(string id, string title, string key, bool useLocalization)
    {
        this.Id = id;
        this.Title = title;
        this.Key = key;
        this.UseLocalization = useLocalization;
    }

    /// <summary>
    /// Gets the id of the menu.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    public List<MenuItem> Items { get; protected set; } = new();

    /// <summary>
    /// Gets the title of the menu.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets a value indicating whether this menu's Title should be used as a key for localizations.
    /// </summary>
    public bool UseLocalization { get; }
}