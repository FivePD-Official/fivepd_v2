using FivePD.Common.Models;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Responsible for <see cref="CommonFVehicle"/> related tasks.
/// </summary>
public interface IVehicleService
{
    /// <summary>
    /// Tries to get a <see cref="CommonFVehicle"/> by its network ID.
    /// </summary>
    /// <param name="networkId">The ped's network ID.</param>
    /// <returns>The ped or null.</returns>
    public CommonFVehicle TryGetVehicle(int networkId);
}