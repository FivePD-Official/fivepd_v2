namespace FivePD.Server.Models.Requests;

#pragma warning disable

internal record struct CreateDivisionRequest
{
    public int DepartmentId { get; set; }

    public string Name { get; set; }
}