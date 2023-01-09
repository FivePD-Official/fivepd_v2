#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Models.Requests
{
    public class CreateDivisionRequest
    {
        public int DepartmentId { get; set; }

        public string Name { get; set; }
    }
}