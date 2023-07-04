using CitizenFX.Core;
using FivePD.Gamemode.Common.Models;

namespace FivePD.Client.Interfaces;

/// <summary>
/// Handles all ped related features.
/// </summary>
internal interface IPedService
{
    /// <summary>
    /// If the player is aiming stops the targeted ped, otherwise the closest one to the player.
    /// </summary>
    /// <param name="ped">The ped to stop.</param>
    public void PedLeaveVehicleWithHandsUp(Ped ped);

    /// <summary>
    /// Generates a ped's <see cref="CommonFPed"/> object.
    /// </summary>
    /// <param name="ped">The ped that's data need to be generated and stored on the server.</param>
    public void GeneratePed(Ped ped);
}