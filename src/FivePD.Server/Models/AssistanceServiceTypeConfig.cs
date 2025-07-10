using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CitizenFX.Core;

namespace FivePD.Server.Models;

#pragma warning disable

[Table("fivepd_assistance_service_type_configs")]
public class AssistanceServiceTypeConfig
{
    public int Id { get; set; }

    public string TypeName { get; set; }

    public string AssemblyName { get; set; }

    public bool Active { get; set; }

    public bool Enabled { get; set; }

    public string SpawnType { get; set; } = "AUTO"; // AUTO | MANUAL | STATION

    public List<Vector3> ManualSpawnLocations { get; set; }

    public bool SpawnTypeAuto()
    {
        return this.SpawnType.ToUpper() == "AUTO";
    }

    public bool SpawnTypeManual()
    {
        return this.SpawnType.ToUpper() == "MANUAL";
    }

    public bool SpawnTypeStation()
    {
        return this.SpawnType.ToUpper() == "STATION";
    }
}