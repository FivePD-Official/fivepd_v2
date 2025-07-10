using CitizenFX.Core;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct CreateAssistanceServiceStationRequest
{
    public string Name { get; set; }
    
    public Vector3 Position { get; set; }
}