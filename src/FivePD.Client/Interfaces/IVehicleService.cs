using CitizenFX.Core;
using FivePD.Client.Models;
using FivePD.Common.Models;

namespace FivePD.Client.Interfaces;

/// <summary>
/// Handles all vehicle related features.
/// </summary>
internal interface IVehicleService
{
    /// <summary>
    /// Generates a vehicle's <see cref="CommonFVehicle"/> object.
    /// </summary>
    /// <param name="vehicle">The vehicle that's data need to be generated and stored on the server.</param>
    /// <returns>The vehicle.</returns>
    public FVehicle GenerateVehicle(Vehicle vehicle);
}