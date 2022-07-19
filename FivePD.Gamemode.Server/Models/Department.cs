// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;

namespace FivePD.Gamemode.Server.Models
{
    /// <summary>
    /// Represents the 'departments' table.
    /// </summary>
    public class Department
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

        /// <summary>
        /// Gets or sets the users of this department.
        /// </summary>
        public IEnumerable<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the divisions of this department.
        /// </summary>
        public IEnumerable<Division> Divisions { get; set; }

        /// <summary>
        /// Gets or sets the ranks of this department.
        /// </summary>
        public IEnumerable<Rank> Ranks { get; set; }
    }
}