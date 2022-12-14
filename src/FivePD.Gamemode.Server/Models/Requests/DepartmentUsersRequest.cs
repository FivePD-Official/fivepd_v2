namespace FivePD.Gamemode.Server.Models.Requests
{
    /// <summary>
    /// The definition of a request for a department's users.
    /// </summary>
    public class DepartmentUsersRequest
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
        public DepartmentUsersRequestFilters Filters { get; set; } = new DepartmentUsersRequestFilters();

        /// <summary>
        /// Contains all filters that is applicable to the given class.
        /// </summary>
        public class DepartmentUsersRequestFilters
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }
        }
    }
}