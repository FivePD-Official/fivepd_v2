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
    /// Gets or sets the assistance service type configuration.
    /// </summary>
    public AssistanceServiceTypeConfig TypeConfig { get; set; }

    /// <summary>
    /// Gets or sets the capacity of the assistance service station type.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Gets or sets the active capacity of the assistance service station type.
    /// </summary>
    public int ActiveCapacity { get; set; }
}