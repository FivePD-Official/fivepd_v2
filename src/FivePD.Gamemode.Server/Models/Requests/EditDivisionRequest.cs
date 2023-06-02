#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests;

public record struct EditDivisionRequest
{
    public int DepartmentId { get; set; }

    public int DivisionId { get; set; }

    public string Name { get; set; }
}