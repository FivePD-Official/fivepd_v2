// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.Interfaces
{
    /**
     * <summary>Provides access to player data,
     * including respective events and methods
     * for modifying a player.</summary>
     */
    public interface IPlayerDataService
    {
        /**
         * <summary>Fires when a player's duty status changes.</summary>
         */
        public event EventHandler<DutyStatusChangedEventArgs> DutyStatusChanged;

        /// <summary>
        /// Fires when a player's rank has been changed.
        /// </summary>
        public event EventHandler<Rank> PlayerRankChanged;

        /// <summary>
        /// Fires when a player's division has been changed.
        /// </summary>
        public event EventHandler<Division> PlayerDivisionChanged;

        /**
         * <summary>Gets the current <see cref="FDutyStatus"/>
         * of the player.</summary>
         * <param name="player">The player to check.</param>
         * <returns>The player's duty status.</returns>
         */
        public FDutyStatus GetDutyStatus(FPlayer player);

        /**
         * <summary>Sets the current <see cref="FDutyStatus"/>
         * of the player.</summary>
         * <param name="player">The player to modify.</param>
         * <param name="dutyStatus">The new duty status.</param>
         */
        public void SetDutyStatus(FPlayer player, FDutyStatus dutyStatus);
    }

    /**
     * <summary>Provides additional information on the
     * player's network identifier and new status.</summary>
     */
    public class DutyStatusChangedEventArgs : EventArgs
    {
        /**
         * <summary>Gets or sets the player
         * affected by the change.</summary>
         */
        public FPlayer Player { get; set; }

        /**
         * <summary>Gets or sets the player's new
         * <see cref="FDutyStatus"/>.</summary>
         */
        public FDutyStatus DutyStatus { get; set; }
    }
}