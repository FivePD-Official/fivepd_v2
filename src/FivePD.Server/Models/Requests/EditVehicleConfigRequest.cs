using System.Collections.Generic;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct EditVehicleConfigRequest
{
    public int Id { get; set; }
    
    public VehicleConfigType Type { get; set; }
    
    public string Name { get; set; }

    public string Model { get; set; }

    public int Livery { get; set; }

    public List<int> Extras { get; set; }

    public Dictionary<string, object> Options { get; set; }
}