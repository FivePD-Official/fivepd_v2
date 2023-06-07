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
    /// Gets the /Config/vehicles.json file's content.
    /// </summary>
    public Vehicles VehiclesConfig { get; }

    /// <summary>
    /// Gets the /Config/menu.json file's content.
    /// </summary>
    public List<Loadout> LoadoutsConfig { get; }

    /// <summary>
    /// Gets the /Config/menu.json file's content.
    /// </summary>
    public MenuConfig MenuConfig { get; }
}