#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests;

public record struct DepartmentDivisionsRequest()
{
    public int DepartmentId { get; set; }

    public int Page { get; set; }

    public DepartmentDivisionsRequestFilters Filters { get; set; } = default;

    public record struct DepartmentDivisionsRequestFilters
    {
        public string Name { get; set; }
    }
}