#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct DepartmentRanksRequest()
{
    public int DepartmentId { get; set; }

    public int Page { get; set; }

    public DepartmentRanksRequestFilters Filters { get; set; } = default;

    public record struct DepartmentRanksRequestFilters
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }
}