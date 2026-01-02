namespace FivePD.Server.Models;

/// <summary>
/// Represents a location type to be used in the marker UI.
/// </summary>
public enum LocationType
{
    /// <summary>
    /// A location where players can access the MDT.
    /// </summary>
    MdtAccess,

    /// <summary>
    /// A location where players can drop off arrested peds.
    /// </summary>
    JailDropOff,

    /// <summary>
    /// A location where players can select their loadout.
    /// </summary>
    LoadoutSelector,

    /// <summary>
    /// A location where players can select their vehicle.
    /// </summary>
    VehicleSelector,
}