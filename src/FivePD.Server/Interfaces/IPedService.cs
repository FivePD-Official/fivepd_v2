using FivePD.Common.Models;
using FivePD.Server.EntityWrappers.PedWrapper;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for <see cref="FPed"/> and <see cref="CommonFPed"/> related tasks.
/// </summary>
public interface IPedService
{
    /// <summary>
    /// Tries to get a <see cref="FPed"/> by its network ID.
    /// </summary>
    /// <param name="networkId">The ped's network ID.</param>
    /// <returns>The ped or null.</returns>
    public FPed TryGetPed(int networkId);
}