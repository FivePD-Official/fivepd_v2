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
    /// <summary>
    /// Represents a normal menu item in the Nui.
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItem"/> class.
        /// </summary>
        /// <param name="title">The title of this item.</param>
        /// <param name="description">The description of this item.</param>
        /// <param name="type">The type of this item.</param>
        /// <param name="onPress">The method that will be invoked upon selecting this item.</param>
        public MenuItem(string title, string description, MenuItemType type, Action<ExpandoObject, CallbackDelegate> onPress)
        {
            this.Title = title;
            this.Description = description;
            this.Type = type;
            this.OnPress = onPress;
        }

        /// <summary>
        /// The type of the item.
        /// </summary>
        public enum MenuItemType
        {
            /// <summary>
            /// Will only trigger an action on press.
            /// </summary>
            Button,

            /// <summary>
            /// Will display a select from which the user can select a value.
            /// </summary>
            List,

            /// <summary>
            /// Will display a checkbox.
            /// </summary>
            Toggle,
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public MenuItemType Type { get; }

        /// <summary>
        /// Gets the action that will be invoked on press.
        /// </summary>
        public Action<ExpandoObject, CallbackDelegate> OnPress { get; }
    }
}