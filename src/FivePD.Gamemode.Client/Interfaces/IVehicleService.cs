using CitizenFX.Core;
using FivePD.Common.Models;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Handles all vehicle related features.
    /// </summary>
    public interface IVehicleService
    {
        /// <summary>
        /// Generates a vehicle's <see cref="CommonFVehicle"/> object.
        /// </summary>
        /// <param name="vehicle">The vehicle that's data need to be generated and stored on the server.</param>
        public void GenerateVehicle(Vehicle vehicle);
    }
}