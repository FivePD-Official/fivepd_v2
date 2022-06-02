// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;

namespace FivePD.Gamemode.Client.Models
{
    /// <inheritdoc />
    public class MenuCheckboxItem : MenuItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuCheckboxItem"/> class.
        /// </summary>
        /// <param name="title">The title of this item.</param>
        /// <param name="description">The description of this item.</param>
        /// <param name="onPress">The method that will be invoked upon selecting this item.</param>
        public MenuCheckboxItem(string title, string description, Action onPress)
            : base(title, description, MenuItemType.Toggle, null)
        {
            this.OnPress = _ =>
            {
                this.Checked = !this.Checked;
                onPress();
            };
        }

        /// <summary>
        /// Gets a value indicating whether this checkbox is checked.
        /// </summary>
        public bool Checked { get; private set; }
    }
}