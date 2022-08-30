// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

namespace FivePD.Common.Models
{
    /// <summary>
    /// The base entity class for
    /// the gamemode.
    /// </summary>
    public class FEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FEntity"/> class.
        /// </summary>
        /// <param name="networkId">The entity's network ID.</param>
        protected FEntity(int networkId)
        {
            this.NetworkId = networkId;
        }

        /// <summary>
        /// Gets the entity's network identifier.
        /// </summary>
        public int NetworkId { get; }
    }
}