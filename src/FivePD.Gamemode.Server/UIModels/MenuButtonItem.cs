// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Common.Models;
using FivePD.Gamemode.Server.UIModels.Interfaces;

#pragma warning disable S3264
#pragma warning disable CS0067

namespace FivePD.Gamemode.Server.UIModels
{
    /// <inheritdoc cref="FivePD.Gamemode.Server.UIModels.MenuItem" />
    public class MenuButtonItem : MenuItem, IMenuButtonItemWithEvents
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuButtonItem"/> class.
        /// </summary>
        /// <param name="title">The title of this item.</param>
        /// <param name="description">The description of this item.</param>
        public MenuButtonItem(string title, string description)
            : base(title, description, MenuItemType.Button)
        {
        }

        /// <summary>
        /// Used for firing the item selection event.
        /// </summary>
        /// <param name="player">The player who invoked this event.</param>
        public delegate void ButtonSelect(FPlayer player);

        /// <summary>
        /// Fires when the item has been selected.
        /// </summary>
        public event ButtonSelect OnSelect;

        /// <inheritdoc/>
        void IMenuButtonItemWithEvents.RaiseOnSelect(FPlayer player)
        {
            this.OnSelect?.Invoke(player);
        }
    }
}