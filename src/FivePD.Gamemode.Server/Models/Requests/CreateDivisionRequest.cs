#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests
{
    public record struct CreateDivisionRequest
    {
        public int DepartmentId { get; set; }

        public string Name { get; set; }
    }
}