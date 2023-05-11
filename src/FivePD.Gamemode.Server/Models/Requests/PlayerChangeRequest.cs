#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests
{
    public record struct PlayerChangeRequest
    {
        public int UserId { get; set; }

        public int IdToChange { get; set; }
    }
}