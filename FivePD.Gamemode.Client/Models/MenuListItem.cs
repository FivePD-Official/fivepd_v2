// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;

namespace FivePD.Gamemode.Client.Models
{
    /// <inheritdoc />
    public class MenuListItem : MenuItem
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
        /// Used for firing the selected item change event.
        /// </summary>
        /// <param name="index">The currently displayed item's index.</param>
        public delegate void ListIndexChange(int index);

        /// <summary>
        /// Used for firing the item selection event.
        /// </summary>
        /// <param name="index">The selected item's index.</param>
        public delegate void ListItemSelect(int index);

        /// <summary>
        /// Fires when the selected item in the list changes.
        /// </summary>
        public event ListIndexChange OnListIndexChange;

        /// <summary>
        /// Fires when the item has been selected.
        /// </summary>
        public event ListItemSelect OnListItemSelect;

        /// <summary>
        /// Gets the items.
        /// </summary>
        public List<string> Items { get; }

        /// <summary>
        /// Gets the currently displayed item's index.
        /// </summary>
        public int CurrentIndex { get; private set; }

        private void RaiseOnListIndexChange(int index)
        {
            this.CurrentIndex = index;
            this.OnListIndexChange?.Invoke(index);
        }

        private void RaiseOnListItemSelect(int index)
        {
            this.OnListItemSelect?.Invoke(index);
        }
    }
}