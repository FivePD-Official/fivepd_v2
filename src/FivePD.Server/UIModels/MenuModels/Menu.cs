using System.Collections.Generic;
using CitizenFX.Core;

namespace FivePD.Server.UIModels.MenuModels;

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
    /// <param name="useLocalization">Determines whether the title and description should be localized.</param>
    protected Menu(string id, string title, string key, bool useLocalization = false)
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
    /// Gets the items.
    /// </summary>
    public List<MenuItem> Items { get; private set; } = new();

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

    /// <summary>
    /// Creates the items.
    /// </summary>
    internal void CreateAndSetItems()
    {
        this.Items = this.CreateItems();
    }

    /// <summary>
    /// Creates the items.
    /// </summary>
    /// <returns>The items.</returns>
    protected abstract List<MenuItem> CreateItems();
}