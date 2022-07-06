// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;

namespace FivePD.Gamemode.Server.Models.ResponseModels
{
    /// <summary>
    /// A data transfer class for the <see cref="Department"/> class
    /// which is used only in the NUI for displaying data.
    /// </summary>
    public class DepartmentResponse
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the acronym.
        /// </summary>
        public string Acronym { get; set; }

        /// <summary>
        /// Gets or sets the callsign format.
        /// </summary>
        public string CallsignFormat { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of users.
        /// </summary>
        public int MaxUsers { get; set; }
    }
}