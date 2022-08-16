// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Common.Models;

namespace FivePD.Gamemode.Client.Interfaces
{
    /**
     * <summary>Provides utility methods for controlling
     * the content presented on user's screen passively.</summary>
     */
    public interface IScreenDecorService
    {
        /// <summary>
        /// Gets or sets the version code to draw.
        /// </summary>
        /// <value>
        ///     A string representation of
        ///     the version (e.x. v2.0.0).
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the duty status to draw.
        /// </summary>
        /// <value>
        ///     The <see cref="FDutyStatus"/>
        ///     of the current player.
        /// </value>
        public FDutyStatus DutyStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether
        /// the on screen build protection notice
        /// is enabled.
        /// </summary>
        /// <value>
        ///     If the protection notice is shown.
        /// </value>
        public bool ProtectionNotice { get; set; }

        /**
         * <summary>Begins drawing text on the screen.</summary>
         */
        public void BeginTextDraw();

        /**
         * <summary>Ends drawing text on the screen.</summary>
         */
        public void EndTextDraw();
    }
}