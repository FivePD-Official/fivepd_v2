#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Models.Requests
{
    public class NameTakenRequest
    {
        public string Name { get; set; }

        public int DepartmentId { get; set; }

        public bool CheckForDepartmentId { get; set; }
    }
}