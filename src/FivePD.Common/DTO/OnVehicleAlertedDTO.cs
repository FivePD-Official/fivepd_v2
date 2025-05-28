using System.Collections.Generic;
using FivePD.Common.Attributes;

namespace FivePD.Common.DTO;

#pragma warning disable

[Obfuscate]
internal sealed record OnVehicleAlertedDTO
{
    public int VehicleNetworkId { get; init; }
    
    public int DriverPedNetworkId { get; init; }
    
    public List<int> OccupantNetworkIds { get; init; }
    
    public bool AlertedOnFoot { get; init; }
    
    public bool CreateTrafficStopInstance { get; init; }
}