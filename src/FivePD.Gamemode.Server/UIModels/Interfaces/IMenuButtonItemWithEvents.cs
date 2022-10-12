// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Common.Models;

namespace FivePD.Gamemode.Server.UIModels.Interfaces
{
    /// <summary>
    /// asd.
    /// </summary>
    public interface IMenuButtonItemWithEvents
    {
        /// <summary>
        /// qwe.
        /// </summary>
        /// <param name="player">Player.</param>
        void RaiseOnSelect(FPlayer player);
    }
}