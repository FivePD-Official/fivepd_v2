namespace FivePD.Server.Models;

/// <summary>
/// Represents the configuration for an assistance service station type.
/// </summary>
public class AssistanceServiceStationTypeConfig
{
    /// <summary>
    /// Gets or sets the unique identifier for the assistance service station type configuration.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the id of the type config.
    /// </summary>
    public int TypeConfigId { get; set; }

    /// <summary>
    /// Gets or sets the assistance service type configuration.
    /// </summary>
    public AssistanceServiceTypeConfig TypeConfig { get; set; }

    /// <summary>
    /// Gets or sets the id of the station.
    /// </summary>
    public int AssistanceServiceStationId { get; set; }

    /// <summary>
    /// Gets or sets the associated assistance service station.
    /// </summary>
    public AssistanceServiceStation AssistanceServiceStation { get; set; }

    /// <summary>
    /// Gets or sets the capacity of the assistance service station type.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Gets or sets the active capacity of the assistance service station type.
    /// </summary>
    public int ActiveCapacity { get; set; }
}