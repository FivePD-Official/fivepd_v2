// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using FivePD.Common.Models;
using FivePD.Common.UIModels.Interfaces;

namespace FivePD.Common.UIModels
{
    /// <inheritdoc cref="FivePD.Common.UIModels.MenuItem" />
    public class MenuListItem : MenuItem, IMenuListItemWithEvents
    {
        private List<string> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuListItem"/> class.
        /// </summary>
        /// <param name="title">The title of this item.</param>
        /// <param name="description">The description of this item.</param>
        /// <param name="items">The items of this list.</param>
        public MenuListItem(string title, string description, List<string> items)
            : base(title, description, MenuItemType.List)
        {
            this._items = items;
        }

        /// <summary>
        /// Used for firing the selected item change event.
        /// </summary>
        /// <param name="player">The player who invoked this event.</param>
        /// <param name="index">The currently displayed item's index.</param>
        public delegate void ListIndexChange(FPlayer player, int index);

        /// <summary>
        /// Used for firing the item selection event.
        /// </summary>
        /// <param name="player">The player who invoked this event.</param>
        /// <param name="index">The selected item's index.</param>
        public delegate void ListSelect(FPlayer player, int index);

        /// <summary>
        /// Fires when the selected item in the list changes.
        /// </summary>
        public event ListIndexChange OnIndexChange;

        /// <summary>
        /// Fires when the item has been selected.
        /// </summary>
        public event ListSelect OnSelect;

        /// <summary>
        /// Gets the items.
        /// </summary>
        public List<string> Items => this._items;

        /// <summary>
        /// Gets the index.
        /// </summary>
        public int CurrentIndex { get; private set; }

        /// <summary>
        /// Sets the list's items.
        /// </summary>
        /// <param name="items">The new items of the list.</param>
        public void SetItems(List<string> items)
        {
            this._items = items;
            this.UpdateItem("items", this._items);
        }

        /// <inheritdoc />
        void IMenuListItemWithEvents.RaiseOnIndexChange(FPlayer player, int index)
        {
            this.CurrentIndex = index;
            this.OnIndexChange?.Invoke(player, index);
        }

        /// <inheritdoc />
        void IMenuListItemWithEvents.RaiseOnSelect(FPlayer player, int index)
        {
            this.OnSelect?.Invoke(player, index);
        }
    }
}