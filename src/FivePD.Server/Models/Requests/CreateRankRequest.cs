#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct CreateRankRequest
{
    public int DepartmentId { get; set; }

    public int RankId { get; set; }

    public string Name { get; set; }

    public Rank.Permissions Permission { get; set; }

    public int RequiredXp { get; set; }
}