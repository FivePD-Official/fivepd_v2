// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Responsible for registering and triggering all events related to radial menus.
    /// </summary>
    public interface IRadialMenuService
    {
        /// <summary>
        /// Registers the official menus.
        /// </summary>
        public void RegisterMenus();
    }
}