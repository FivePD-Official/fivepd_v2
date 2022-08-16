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
        /// <summary>
        /// Fires when a player's rank has been changed.
        /// </summary>
        public event EventHandler<Rank> PlayerRankChanged;

        /// <summary>
        /// Fires when a player's division has been changed.
        /// </summary>
        public event EventHandler<Division> PlayerDivisionChanged;
    }
}