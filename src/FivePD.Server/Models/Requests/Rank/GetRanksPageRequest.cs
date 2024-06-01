#pragma warning disable

namespace FivePD.Server.Models.Requests.Rank;

internal record struct GetRanksPageRequest()
{
    public int Page { get; set; }

    public DepartmentRanksRequestFilters Filters { get; set; } = default;

    public record struct DepartmentRanksRequestFilters
    {
        public string Name { get; set; }

        public bool Global { get; set; }

        public int DepartmentId { get; set; }
    }
}