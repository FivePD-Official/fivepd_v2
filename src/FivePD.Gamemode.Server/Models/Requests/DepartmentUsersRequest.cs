#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests
{
    public record struct DepartmentUsersRequest()
    {
        public int DepartmentId { get; set; }

        public int Page { get; set; }

        public DepartmentUsersRequestFilters Filters { get; set; } = default;

        public record struct DepartmentUsersRequestFilters
        {
            public string Name { get; set; }
        }
    }
}