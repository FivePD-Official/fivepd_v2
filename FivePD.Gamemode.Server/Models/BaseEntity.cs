// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

namespace FivePD.Gamemode.Server.Models
{
    /// <summary>
    /// Contains utility methods for entities.
    /// </summary>
    /// <typeparam name="T">The type of the table's corresponding data transfer class.</typeparam>
#pragma warning disable SA1649
    public abstract class BaseEntity<T>
#pragma warning restore SA1649
    {
        /// <summary>
        /// Creates a data transfer object
        /// from the current instance.
        /// </summary>
        /// <returns>The created data transfer object.</returns>
        public abstract T ToResponse();
    }
}