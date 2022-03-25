// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Dynamic;
using CitizenFX.Core;

namespace FivePD.Gamemode.Client.Models
{
    /// <inheritdoc />
    public class MenuItemToggle : MenuItem
    {
        /// <summary>
        /// The value indicating whether this checkbox is checked.
        /// </summary>
        private bool _isChecked;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItemToggle"/> class.
        /// </summary>
        /// <param name="title">The title of this item.</param>
        /// <param name="description">The description of this item.</param>
        /// <param name="onPress">The method that will be invoked upon selecting this item.</param>
        public MenuItemToggle(string title, string description, Action<int, CallbackDelegate> onPress)
            : base(title, description, MenuItemType.Toggle, onPress)
        {
        }

        private void Toggle()
        {
            this._isChecked = !this._isChecked;
        }
    }
}