// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

#pragma warning disable S3264
#pragma warning disable CS0067

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
        public MenuCheckboxItem(string title, string description)
            : base(title, description, MenuItemType.Toggle)
        {
        }

        /// <summary>
        /// Fires when the item has been selected.
        /// </summary>
        public event ItemSelect OnItemSelect;

        /// <summary>
        /// Gets a value indicating whether this checkbox is checked.
        /// </summary>
        public bool Checked { get; private set; }

        private void RaiseOnItemSelect()
        {
            this.Checked = !this.Checked;
            this.OnItemSelect?.Invoke();
        }
    }
}