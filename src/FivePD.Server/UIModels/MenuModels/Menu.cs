using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers;

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
    protected Menu(string id, string title, string key)
    {
        this.Id = id;
        this.Title = title;
        this.Key = key;
    }

    /// <summary>
    /// Gets the id of the menu.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the title of the menu.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets the items.
    /// </summary>
    public IReadOnlyList<MenuItem> Items => this.InternalItems;

    /// <summary>
    /// Gets the items.
    /// </summary>
    internal List<MenuItem> InternalItems { get; private set; } = new();

    /// <summary>
    /// Invoked upon pressing the key of the menu.
    /// The menu will be opened for the player only if the return value is true.
    /// </summary>
    /// <param name="player">The player who initiated the opening of the menu.</param>
    /// <returns>A value indicating whether the menu should be opened or not.</returns>
    public abstract bool CanOpen(FPlayer player);

    /// <summary>
    /// Creates the items.
    /// </summary>
    internal void CreateAndSetItems()
    {
        this.InternalItems = this.CreateItems();
    }

    /// <summary>
    /// Creates the items.
    /// </summary>
    /// <returns>The items.</returns>
    protected abstract List<MenuItem> CreateItems();
}