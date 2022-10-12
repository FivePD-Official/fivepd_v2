// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using CitizenFX.Core;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.UIModels.Interfaces;

#pragma warning disable S3264
#pragma warning disable CS0067

namespace FivePD.Gamemode.Server.UIModels
{
    /// <inheritdoc cref="FivePD.Gamemode.Server.UIModels.MenuItem" />
    public class MenuCheckboxItem : MenuItem, IMenuCheckboxItemWithEvents
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuCheckboxItem"/> class.
        /// </summary>
        /// <param name="title">The title of this item.</param>
        /// <param name="description">The description of this item.</param>
        public MenuCheckboxItem(string title, string description)
            : base(title, description, MenuItemType.Checkbox)
        {
        }

        /// <summary>
        /// Used for firing the item selection event.
        /// </summary>
        /// <param name="player">The player who invoked this event.</param>
        /// <param name="checked">The player who2 invoked this event.</param>
        public delegate void CheckboxSelect(FPlayer player, bool @checked);

        /// <summary>
        /// Fires when the item has been selected.
        /// </summary>
        public event CheckboxSelect OnSelect;

        /// <inheritdoc/>
        void IMenuCheckboxItemWithEvents.RaiseOnSelect(FPlayer player, bool @checked)
        {
            this.OnSelect?.Invoke(player, @checked);
        }
    }
}