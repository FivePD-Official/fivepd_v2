using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CitizenFX.Core;

namespace FivePD.Server.Models;

/// <summary>
/// Represents an assistance service station.
/// </summary>
[Table("fivepd_assistance_service_stations")]
public class AssistanceServiceStation
{
    /// <summary>
    /// Gets or sets the unique identifier of the assistance service station.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the assistance service station.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the position of the assistance service station.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Gets or sets the type configurations for the assistance service station.
    /// </summary>
    public IList<AssistanceServiceStationTypeConfig> TypeConfigs { get; set; }
}