// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;

namespace FivePD.Gamemode.Client.Models
{
    /// <inheritdoc />
    public class MenuButtonItem : MenuItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuButtonItem"/> class.
        /// </summary>
        /// <param name="title">The title of this item.</param>
        /// <param name="description">The description of this item.</param>
        /// <param name="onPress">The method that will be invoked upon selecting this item.</param>
        public MenuButtonItem(string title, string description, Action onPress)
            : base(title, description, MenuItemType.Button, _ => onPress())
        {
        }
    }
}