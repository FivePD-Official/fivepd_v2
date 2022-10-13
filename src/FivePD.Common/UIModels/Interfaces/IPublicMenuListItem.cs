// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;

namespace FivePD.Common.UIModels.Interfaces
{
    /// <summary>
    /// asd.
    /// </summary>
    public interface IPublicMenuListItem
    {
        /// <summary>
        /// Gets the items.
        /// </summary>
        public List<string> Items { get; }

        /// <summary>
        /// Gets the currently selected index.
        /// </summary>
        public int CurrentIndex { get; }
    }
}