using System.Collections.Generic;
using FivePD.Gamemode.Common.ConfigModels;

namespace FivePD.Gamemode.Client.Interfaces;

/// <summary>
/// Provides utility methods for the FivePD resource itself.
/// </summary>
internal interface IConfigService
{
    /// <summary>
    /// Gets the content of the "Config/general-settings.json" file.
    /// </summary>
    public GeneralSettings GeneralSettings { get; }

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