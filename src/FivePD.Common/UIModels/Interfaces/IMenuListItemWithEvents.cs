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
    public interface IMenuListItemWithEvents
    {
        /// <summary>
        /// qweqwe.
        /// </summary>
        /// <param name="player">A.</param>
        /// <param name="index">B.</param>
        public void RaiseOnIndexChange(FPlayer player, int index);

        /// <summary>
        /// qweqwe2.
        /// </summary>
        /// <param name="player">A.</param>
        /// <param name="index">B.</param>
        void RaiseOnSelect(FPlayer player, int index);
    }
}