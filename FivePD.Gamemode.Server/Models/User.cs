// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Gamemode.Server.Models.ResponseModels;

namespace FivePD.Gamemode.Server.Models
{
    /// <summary>
    /// Represents the 'users' table.
    /// </summary>
    public class User : BaseEntity<UserResponse>
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
        /// Gets or sets the license key.
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is an admin.
        /// </summary>
        public bool Admin { get; set; }

        /// <summary>
        /// Gets or sets the callsign.
        /// </summary>
        public string Callsign { get; set; }

        /// <summary>
        /// Gets or sets the department which the user is joined to.
        /// </summary>
        public Department Department { get; set; }

        /// <summary>
        /// Gets or sets the division which the user is joined to.
        /// </summary>
        public Division Division { get; set; }

        /// <summary>
        /// Gets or sets the rank.
        /// </summary>
        public Rank Rank { get; set; }

        /// <inheritdoc />
        public override UserResponse ToResponse()
        {
            return new UserResponse
            {
                Id = this.Id,
                Name = this.Name,
                License = this.License,
                Admin = this.Admin,
                Callsign = this.Callsign,
                Department = this.Department?.ToResponse(),
                Division = this.Division?.ToResponse(),
                Rank = this.Rank?.ToResponse(),
            };
        }
    }
}