#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Models.Requests
{
    public class EditRankRequest
    {
        public int DepartmentId { get; set; }

        public int RankId { get; set; }

        public string Name { get; set; }

        public Rank.Permissions Permission { get; set; }
    }
}