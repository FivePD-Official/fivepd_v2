// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;

namespace FivePD.Gamemode.Client.Attributes
{
    /// <inheritdoc />
    public class KeybindAttribute : BaseKeybindAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeybindAttribute"/> class.
        /// </summary>
        /// <param name="id">Identifier of the keybind. For example: "fivepd_open_mdt".</param>
        /// <param name="key">The key's name which will be binded to the given method.</param>
        /// <param name="description">Description of the keybind that will be showed in the settings.</param>
        public KeybindAttribute(string id, string key, string description = "")
            : base(id, key, description, 0)
        {
        }
    }
}