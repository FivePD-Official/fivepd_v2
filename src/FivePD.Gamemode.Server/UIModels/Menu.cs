// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;

namespace FivePD.Gamemode.Server.UIModels
{
    /// <summary>
    /// Defines a UI menu.
    /// </summary>
    public abstract class Menu
    {
        private List<MenuItem> _items = new List<MenuItem>();

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
        /// Gets the items.
        /// </summary>
        public List<MenuItem> Items => this._items;

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
        /// Sets the items.
        /// </summary>
        /// <param name="items">The items of this menu.</param>
        protected void SetItems(List<MenuItem> items)
        {
            this._items = items;
        }
    }
}