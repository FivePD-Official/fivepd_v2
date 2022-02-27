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
        /// Shows NUI.
        /// </summary>
        public void Show();

        /// <summary>
        /// Hides NUI.
        /// </summary>
        public void Hide();

        // TODO: switch between MDT and menu view
    }
}