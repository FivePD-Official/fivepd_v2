using System.Collections.Generic;
using FivePD.Common.ConfigModels;

#pragma warning disable SA1618

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Provides utility methods for the FivePD resource itself.
    /// </summary>
    public interface IConfigService
    {
        /// <summary>
        /// Loads and returns the /Config/vehicles.json file's content.
        /// </summary>
        /// <returns>Returns a deserialized <see cref="Vehicles"/> object.</returns>
        public Vehicles GetVehiclesConfig();

        /// <summary>
        /// Loads and returns the /Config/menu.json file's content.
        /// </summary>
        /// <returns>Returns a deserialized a list of <see cref="Loadout"/> objects.</returns>
        public List<Loadout> GetLoadoutsConfig();

        /// <summary>
        /// Loads and returns the /Config/menu.json file's content.
        /// </summary>
        /// <returns>Returns a deserialized <see cref="MenuConfig"/> object.</returns>
        public MenuConfig GetMenuConfig();
    }
}