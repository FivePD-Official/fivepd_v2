// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;

namespace FivePD.Gamemode.Client.Attributes
{
    /// <summary>
    /// Used to bind methods to given keys.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class KeybindAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeybindAttribute"/> class.
        /// </summary>
        /// <param name="id">Identifier of the keybind. For example: "fivepd_open_mdt".</param>
        /// <param name="key">The key's name which will be binded to the given method.</param>
        public KeybindAttribute(string id, string key)
        {
            this.Id = id;
            this.Key = key;
        }

        /// <summary>
        /// Gets identifier of the binded action.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets key's name to bind.
        /// </summary>
        public string Key { get; }
    }
}