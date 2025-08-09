namespace FivePD.Server.Models;

/// <summary>
/// Represents the type of a vehicle configuration.
/// </summary>
public enum VehicleConfigType
{
    /// <summary>
    /// A police vehicle.
    /// </summary>
    Police,

    /// <summary>
    /// An ambulance vehicle.
    /// </summary>
    Ambulance,

    /// <summary>
    /// A tow truck vehicle.
    /// </summary>
    TowTruck,

    /// <summary>
    /// A ride share vehicle.
    /// </summary>
    RideShare,
}