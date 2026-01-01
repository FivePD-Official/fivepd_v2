using System.Collections.Generic;
using CitizenFX.Core;

namespace FivePD.Server.Models;

#pragma warning disable

/// <summary>
/// Represents an assistance service station.
/// </summary>
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
    public List<AssistanceServiceStationTypeConfig> TypeConfigs { get; set; } = new();
}