#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests
{
    public record struct EditDepartmentRequest()
    {
        public int DepartmentId { get; set; }

        public string Name { get; set; }

        public string Callsign { get; set; } = string.Empty;

        public string CallsignFormat { get; set; }

        public int MaxUsers { get; set; }

        public int DefaultRankId { get; set; }
    }
}