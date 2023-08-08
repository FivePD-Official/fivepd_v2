#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct PlayerChangeFixedRankRequest
{
    public int UserId { get; set; }

    public bool FixedRank { get; set; }
}