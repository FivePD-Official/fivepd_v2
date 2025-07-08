using System.Collections.Generic;
using CitizenFX.Core;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct EditAssistanceServiceTypeConfig
{
    public int Id { get; set; }
    
    public bool Enabled { get; set; }
    
    public string SpawnType { get; set; }
    
    public List<Vector3> ManualSpawnLocations { get; set; }
}