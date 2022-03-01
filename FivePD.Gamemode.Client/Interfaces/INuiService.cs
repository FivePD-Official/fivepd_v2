// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Provides the necessary methods to toggle the NUI
    /// and to switch between MDT views and menus.
    /// </summary>
    public interface INuiService
    {
        /// <summary>
        /// Registers all necessary NUI events for the MDT and menus.
        /// </summary>
        public void RegisterEventHandlers();

        /// <summary>
        /// Hides NUI.
        /// </summary>
        public void Hide();
    }
}