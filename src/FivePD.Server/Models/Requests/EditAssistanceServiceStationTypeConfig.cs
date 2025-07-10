#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct EditAssistanceServiceStationTypeConfig
{
    public int Id { get; set; }
    
    public int Capacity { get; set; }
}