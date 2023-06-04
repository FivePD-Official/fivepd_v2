#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests;

internal record struct EditRankRequest
{
    public int DepartmentId { get; set; }

    public int RankId { get; set; }

    public string Name { get; set; }

    public Rank.Permissions Permission { get; set; }
}