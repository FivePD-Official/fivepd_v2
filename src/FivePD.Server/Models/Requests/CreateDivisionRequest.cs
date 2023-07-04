#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct CreateDivisionRequest
{
    public int DepartmentId { get; set; }

    public string Name { get; set; }
}