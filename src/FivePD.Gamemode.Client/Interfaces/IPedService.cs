using CitizenFX.Core;
using FivePD.Common.Models;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Handles all ped related features.
    /// </summary>
    public interface IPedService
    {
        /// <summary>
        /// Registers all ped related events.
        /// </summary>
        public void RegisterEvents();

        /// <summary>
        /// If the player is aiming stops the targeted ped, otherwise the closest one to the player.
        /// </summary>
        /// <param name="ped">The ped to stop.</param>
        /// <param name="handsUp">A value indicating whether the ped should be given a hands up animation or not.</param>
        public void StopPed(Ped ped, bool handsUp = false);

        /// <summary>
        /// If the player is aiming stops the targeted ped, otherwise the closest one to the player.
        /// </summary>
        /// <param name="ped">The ped to stop.</param>
        public void PedLeaveVehicleWithHandsUp(Ped ped);

        /// <summary>
        /// Generates a ped's <see cref="FPed"/> object.
        /// </summary>
        /// <param name="ped">The ped that's data need to be generated and stored on the server.</param>
        public void GeneratePed(Ped ped);
    }
}