// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using CitizenFX.Core;
using FivePD.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Models
{
    /// <summary>
    /// Represents a normal menu item in the Nui.
    /// </summary>
    public class MenuItem
    {
        private string _title;
        private bool _enabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItem"/> class.
        /// </summary>
        /// <param name="title">The title of this item.</param>
        /// <param name="description">The description of this item.</param>
        /// <param name="type">The type of this item.</param>
        /// <param name="onPress">The method that will be invoked upon selecting this item.</param>
        public MenuItem(string title, string description, MenuItemType type, Action<int, CallbackDelegate> onPress)
        {
            this._title = title;
            this._enabled = true;
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
        /// Gets or sets the title the item.
        /// </summary>
        public string Title
        {
            get => this._title;
            set
            {
                this._title = value;
                this.UpdateItem(new JObject
                {
                    { "type", Events.NuiEventType.UpdateMenuItem },
                    { "hashcode", this.GetHashCode().ToString() },
                    { "fieldname", "title" },
                    { "value", this._title },
                });
            }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is clickable.
        /// </summary>
        public bool Enabled
        {
            get => this._enabled;
            set
            {
                this._enabled = value;
                this.UpdateItem(new JObject
                {
                    { "type", Events.NuiEventType.UpdateMenuItem },
                    { "hashcode", this.GetHashCode().ToString() },
                    { "fieldname", "enabled" },
                    { "value", this._enabled },
                });
            }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public MenuItemType Type { get; }

        /// <summary>
        /// Gets the action that will be invoked on press.
        /// </summary>
        public Action<int, CallbackDelegate> OnPress { get; }

        private void UpdateItem(JObject data)
        {
            Cfx.API.SendNuiMessage(JsonConvert.SerializeObject(data));
        }
    }
}