using System.Collections.Generic;

namespace FivePD.Common.UIModels
{
    /// <summary>
    /// Defines a UI menu.
    /// </summary>
    public abstract class Menu
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Menu"/> class.
        /// </summary>
        /// <param name="title">The title of the menu.</param>
        /// <param name="key">The keybind of this menu.</param>
        /// <param name="useLocalization">Determines whether this menu's Title should be used as a key for localizations.</param>
        protected Menu(string title, string key, bool useLocalization)
        {
            this.Title = title;
            this.Key = key;
            this.UseLocalization = useLocalization;
        }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<MenuItem> Items { get; protected set; } = new List<MenuItem>();

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
}