using FivePD.Gamemode.Common.Models;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.EntityWrappers.VehicleWrapper;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Responsible for <see cref="FVehicle"/> and <see cref="CommonFVehicle"/> related tasks.
/// </summary>
public interface IVehicleService
{
    /// <summary>
    /// Tries to get a <see cref="FVehicle"/> by its network ID.
    /// </summary>
    /// <param name="networkId">The ped's network ID.</param>
    /// <returns>The ped or null.</returns>
    public FVehicle TryGetVehicle(int networkId);
}