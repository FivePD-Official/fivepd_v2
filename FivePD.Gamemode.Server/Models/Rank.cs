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
    public class Rank : BaseEntity<RankResponse>
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

        /// <inheritdoc />
        public override RankResponse ToResponse()
        {
            return new RankResponse
            {
                Id = this.Id,
                Name = this.Name,
            };
        }
    }
}