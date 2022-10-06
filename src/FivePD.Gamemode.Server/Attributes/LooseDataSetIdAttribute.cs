// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;

namespace FivePD.Gamemode.Server.Attributes
{
    /// <summary>
    /// Used for handling the dynamic data sets.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LooseDataSetIdAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LooseDataSetIdAttribute"/> class.
        /// </summary>
        /// <param name="id">The id of the set.</param>
        public LooseDataSetIdAttribute(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the id of the set.
        /// </summary>
        public string Id { get; }
    }
}