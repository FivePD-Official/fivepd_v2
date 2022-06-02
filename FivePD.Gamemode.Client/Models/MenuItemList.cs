// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;

namespace FivePD.Gamemode.Client.Models
{
    /// <inheritdoc />
    public class MenuItemList : MenuItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItemList"/> class.
        /// </summary>
        /// <param name="title">The title of this item.</param>
        /// <param name="description">The description of this item.</param>
        /// <param name="items">The items of this list.</param>
        /// <param name="onPress">The method that will be invoked upon selecting this item.</param>
        public MenuItemList(string title, string description, List<string> items, Action<int> onPress)
            : base(title, description, MenuItemType.List, onPress)
        {
            this.Items = items;
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        public List<string> Items { get; }
    }
}