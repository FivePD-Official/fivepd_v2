namespace FivePD.Gamemode.Server.Models.Requests
{
    /// <summary>
    /// The definition of a request for a department's divisions.
    /// </summary>
    public class DepartmentDivisionsRequest
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
        public DepartmentDivisionsRequestFilters Filters { get; set; } = new DepartmentDivisionsRequestFilters();

        /// <summary>
        /// Contains all filters that is applicable to the given class.
        /// </summary>
        public class DepartmentDivisionsRequestFilters
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }
        }
    }
}