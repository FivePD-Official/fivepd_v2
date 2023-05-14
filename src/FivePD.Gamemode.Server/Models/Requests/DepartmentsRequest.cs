#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests
{
    public record struct DepartmentsRequest()
    {
        public int Page { get; set; }

        public DepartmentRequestFilters Filters { get; set; } = default;

        public record struct DepartmentRequestFilters
        {
            public string Name { get; set; }

            public string Callsign { get; set; }
        }
    }
}