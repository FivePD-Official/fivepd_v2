// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using CitizenFX.Core;

namespace FivePD.Common.Models
{
    /// <summary>
    /// Represents a stateful object
    /// that stores data for a specific
    /// instance of a thing.
    /// </summary>
    public abstract class FStatefulContainer
    {
        /// <summary>
        /// Gets the underlying statebag
        /// key defined by the entity type.
        /// </summary>
        /// <returns>The key for the statebag.</returns>
        public abstract string GetStateKey();

        /// <summary>
        /// Gets the underlying statebag
        /// for this instance.
        /// </summary>
        /// <returns>The statebag.</returns>
        public abstract StateBag GetStateBag();
    }
}