#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct NameTakenRequest
{
    public string Name { get; set; }

    public int DepartmentId { get; set; }

    public bool CheckForDepartmentId { get; set; }
}