// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Gamemode.Server.Models.ResponseModels;

namespace FivePD.Gamemode.Server.Models
{
    /// <summary>
    /// Represents the 'divisions' table.
    /// </summary>
    public class Division : BaseEntity<DivisionResponse>
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
        /// Gets or sets the department of this division.
        /// </summary>
        public Department Department { get; set; }

        /// <inheritdoc />
        public override DivisionResponse ToResponse()
        {
            return new DivisionResponse
            {
                Id = this.Id,
                Name = this.Name,
            };
        }
    }
}