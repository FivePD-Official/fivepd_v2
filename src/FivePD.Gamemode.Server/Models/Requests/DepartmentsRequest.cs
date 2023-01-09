namespace FivePD.Gamemode.Server.Models.Requests
{
    /// <summary>
    /// The definition of a request for all departments.
    /// </summary>
    public class DepartmentsRequest
    {
        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the filters which will be applied to the query.
        /// </summary>
        public DepartmentRequestFilters Filters { get; set; } = new DepartmentRequestFilters();

        /// <summary>
        /// Contains all filters that is applicable to the given class.
        /// </summary>
        public class DepartmentRequestFilters
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the acronym.
            /// </summary>
            public string Callsign { get; set; }
        }
    }
}