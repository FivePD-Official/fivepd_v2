// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Common.Attributes;
using FivePD.Common.Models;

namespace FivePD.Common.UIModels.Interfaces
{
    /// <summary>
    /// asd.
    /// </summary>
    [NoUsage]
    public interface IMenuCheckboxItemWithEvents
    {
        /// <summary>
        /// qwe.
        /// </summary>
        /// <param name="player">Player.</param>
        /// <param name="checked">checked.</param>
        void RaiseOnSelect(FPlayer player, bool @checked);
    }
}