// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using FivePD.Gamemode.Server.Models.ResponseModels;

namespace FivePD.Gamemode.Server.Models
{
    /// <summary>
    /// Represents the 'departments' table.
    /// </summary>
    public class Department : BaseEntity<DepartmentResponse>
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

        /// <inheritdoc />
        public override DepartmentResponse ToResponse()
        {
            return new DepartmentResponse
            {
                Id = this.Id,
                Name = this.Name,
                Acronym = this.Acronym,
                CallsignFormat = this.CallsignFormat,
                MaxUsers = this.MaxUsers,
            };
        }
    }
}