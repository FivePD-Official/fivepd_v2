using CitizenFX.Core;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct CreateLocationRequest
{
    public LocationType Type { get; set; }
    
    public string Name { get; set; }

    public Vector3 Position;
}