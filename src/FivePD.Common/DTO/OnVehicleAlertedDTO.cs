using System.Collections.Generic;

namespace FivePD.Client.DTO;

#pragma warning disable

internal sealed record OnVehicleAlertedDTO
{
    public int VehicleNetworkId { get; init; }
    
    public int DriverPedNetworkId { get; init; }
    
    public List<int> OccupantNetworkIds { get; init; }
    
    public bool AlertedOnFoot { get; init; }
}