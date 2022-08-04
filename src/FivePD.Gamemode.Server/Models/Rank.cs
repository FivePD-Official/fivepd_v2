// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

#pragma warning disable SA1402

namespace FivePD.Gamemode.Server.Models
{
    /// <summary>
    /// Represents the 'divisions' table.
    /// </summary>
    public class Rank
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
        /// Gets or sets the department which it's attached to.
        /// </summary>
        public Department Department { get; set; }

        /// <summary>
        /// Gets or sets the permissions of this rank.
        /// </summary>
        public Permissions Permission { get; set; }

        /// <summary>
        /// Gets or sets the users of this ranks.
        /// </summary>
        public IEnumerable<User> Users { get; set; }

        /// <summary>
        /// Defines the actions a given <see cref="Rank"/> can do.
        /// </summary>
        public class Permissions
        {
            /// <summary>
            /// Gets or sets a value indicating whether the user has the ability
            /// to kick other players from the department.
            /// </summary>
            public bool Kick { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the user has the ability
            /// to set the rank of other players in the department.
            /// </summary>
            public bool SetRank { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the user has the ability
            /// to set the callsign of other players in the department.
            /// </summary>
            public bool SetCallsign { get; set; }
        }
    }
}