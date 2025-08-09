using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CitizenFX.Core;

namespace FivePD.Server.Models;

/// <summary>
/// Represents the configuration for an assistance service type.
/// </summary>
[Table("fivepd_assistance_service_type_configs")]
public class AssistanceServiceTypeConfig
{
    /// <summary>
    /// Gets or sets the unique identifier for the assistance service type configuration.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the type.
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// Gets or sets the name of the assembly.
    /// </summary>
    public string AssemblyName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the assistance service type is active.
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the assistance service type is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the spawn type.
    /// </summary>
    public string SpawnType { get; set; } = "AUTO"; // AUTO | MANUAL | STATION

    /// <summary>
    /// Gets or sets the manual spawn locations.
    /// </summary>
    public List<Vector3> ManualSpawnLocations { get; set; }

    /// <summary>
    /// Determines whether the spawn type is set to "AUTO".
    /// </summary>
    /// <returns><c>true</c> if the spawn type is "AUTO"; otherwise, <c>false</c>.</returns>
    public bool SpawnTypeAuto()
    {
        return this.SpawnType.ToUpper() == "AUTO";
    }

    /// <summary>
    /// Determines whether the spawn type is set to "MANUAL".
    /// </summary>
    /// <returns><c>true</c> if the spawn type is "MANUAL"; otherwise, <c>false</c>.</returns>
    public bool SpawnTypeManual()
    {
        return this.SpawnType.ToUpper() == "MANUAL";
    }

    /// <summary>
    /// Determines whether the spawn type is set to "STATION".
    /// </summary>
    /// <returns><c>true</c> if the spawn type is "STATION"; otherwise, <c>false</c>.</returns>
    public bool SpawnTypeStation()
    {
        return this.SpawnType.ToUpper() == "STATION";
    }
}