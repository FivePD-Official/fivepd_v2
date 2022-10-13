// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

namespace FivePD.Common.UIModels.Interfaces
{
    /// <summary>
    /// qwe.
    /// </summary>
    public interface IPublicMenuItem
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the item is enabled or not.
        /// </summary>
        public bool Enabled { get; }
    }
}