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
    /// Defines the events of <see cref="MenuCheckboxItem"/>.
    /// </summary>
    [NoUsage]
    public interface IMenuCheckboxItemWithEvents
    {
        /// <summary>
        /// Raises the OnSelect event.
        /// </summary>
        /// <param name="player">The player who invoked this event.</param>
        /// <param name="checked">The value of the checkbox.</param>
        void RaiseOnSelect(FPlayer player, bool @checked);
    }
}