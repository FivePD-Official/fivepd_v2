using FivePD.Common.Models;

namespace FivePD.Gamemode.Server.Interfaces
{
    /// <summary>
    /// Responsible for <see cref="CommonFPed"/> related tasks.
    /// </summary>
    public interface IPedService
    {
        /// <summary>
        /// Tries to get a <see cref="CommonFPed"/> by its network ID.
        /// </summary>
        /// <param name="networkId">The ped's network ID.</param>
        /// <returns>The ped or null.</returns>
        public CommonFPed TryGetPed(int networkId);
    }
}