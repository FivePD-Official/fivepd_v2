#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests;

public record struct NameTakenRequest
{
    public string Name { get; set; }

    public int DepartmentId { get; set; }

    public bool CheckForDepartmentId { get; set; }
}