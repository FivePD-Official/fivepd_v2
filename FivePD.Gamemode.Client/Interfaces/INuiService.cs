// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using Newtonsoft.Json.Linq;

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
        /// Opens the test radial menu.
        /// </summary>
        /// <param name="menuId">Identifier of the menu.</param>
        public void ShowRadialMenu(string menuId);

        /// <summary>
        /// Send control event to the NUI.
        /// </summary>
        /// <param name="data">The data to send.</param>
        public void SendMessage(string data);

        /// <summary>
        /// Send control event to the NUI.
        /// </summary>
        /// <param name="data">The data to send.</param>
        public void SendMessage(JObject data);

        /// <summary>
        /// Hides NUI.
        /// </summary>
        public void Hide();

        /// <summary>
        /// Opens the MDT.
        /// </summary>
        public void OpenMdt();

        /// <summary>
        /// Opens the duty menu.
        /// </summary>
        public void OpenDutyMenu();

        /// <summary>
        /// Moves up by one item in the currently opened menu.
        /// </summary>
        public void MenuControlUp();

        /// <summary>
        /// Moves down by one item in the currently opened menu.
        /// </summary>
        public void MenuControlDown();

        /// <summary>
        /// Moves left by one item in the currently opened menu.
        /// </summary>
        public void MenuControlLeft();

        /// <summary>
        /// Moves right by one item in the currently opened menu.
        /// </summary>
        public void MenuControlRight();

        /// <summary>
        /// Selects the current item in the menu.
        /// </summary>
        public void MenuControlSelect();
    }
}