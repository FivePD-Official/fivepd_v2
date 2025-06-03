using CitizenFX.Core;

namespace FivePD.Client.Interfaces;

/// <summary>
/// Responsible for handling vehicle config related functionality.
/// </summary>
internal interface IVehicleConfigService
{
    /// <summary>
    /// Checks if the given vehicle is a police vehicle based on its model.
    /// </summary>
    /// <param name="vehicle">The vehicle to check.</param>
    /// <returns>True if the vehicle is a police vehicle, false otherwise.</returns>
    bool IsPoliceVehicle(Vehicle vehicle);
}
