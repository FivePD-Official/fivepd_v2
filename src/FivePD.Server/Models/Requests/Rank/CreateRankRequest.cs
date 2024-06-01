#pragma warning disable

namespace FivePD.Server.Models.Requests.Rank;

internal record struct CreateRankRequest
{
    public int DepartmentId { get; set; }

    public string Name { get; set; }

    public Models.Rank.Permissions Permission { get; set; }

    public int RequiredXp { get; set; }
}