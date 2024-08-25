#pragma warning disable

namespace FivePD.Server.Models.Requests.Rank;

internal record struct EditGlobalRankRequest
{
    public int RankId { get; set; }

    public string Name { get; set; }

    public Models.Rank.Permissions Permission { get; set; }

    public int RequiredXp { get; set; }
}