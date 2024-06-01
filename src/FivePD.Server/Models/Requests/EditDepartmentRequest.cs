#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct EditDepartmentRequest()
{
    public int DepartmentId { get; set; }

    public string Name { get; set; }

    public string Callsign { get; set; } = string.Empty;

    public int MaxUsers { get; set; }

    public int DefaultRankId { get; set; }
    
    public bool UseGlobalRanks { get; set; }   
}