// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Gamemode.Client.Injection;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Used to register keybindings.
    /// </summary>
    public interface IKeybindService
    {
        /// <summary>
        /// Finds all methods with the Keybind attribute
        /// and register them.
        /// </summary>
        /// <param name="provider">The <see cref="ServiceProvider"/> to be used.</param>>
        public void Register(ServiceProvider provider);
    }
}