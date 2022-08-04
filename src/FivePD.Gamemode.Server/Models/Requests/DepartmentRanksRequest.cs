// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

namespace FivePD.Gamemode.Server.Models.Requests
{
    /// <summary>
    /// The definition of a request for a department's ranks.
    /// </summary>
    public class DepartmentRanksRequest
    {
        /// <summary>
        /// Gets or sets the department id.
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the filters which will be applied to the query.
        /// </summary>
        public DepartmentRanksRequestFilters Filters { get; set; } = new DepartmentRanksRequestFilters();

        /// <summary>
        /// Contains all filters that is applicable to the given class.
        /// </summary>
        public class DepartmentRanksRequestFilters
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }
        }
    }
}