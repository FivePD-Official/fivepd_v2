using CitizenFX.Core;

namespace FivePD.Server.Models.AssistanceServiceModels;

/// <summary>
/// Represents the configuration options for a tow truck vehicle.
/// </summary>
public class TowTruckVehicleConfigOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the siren is active.
    /// </summary>
    public bool SirenActive { get; set; }

    /// <summary>
    /// Gets or sets the offset for the towed vehicle's position.
    /// </summary>
    public Vector3 TowedVehiclePositionOffset { get; set; }
}