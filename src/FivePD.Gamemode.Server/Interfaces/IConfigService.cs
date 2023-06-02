using System.Collections.Generic;
using FivePD.Common.Attributes;
using FivePD.Common.ConfigModels;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Provides utility methods for the FivePD resource itself.
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// Loads and returns the /Config/items.json file's content.
    /// </summary>
    /// <returns>Returns all items from the config file.</returns>
    public List<Item> GetItems();

    /// <summary>
    /// Loads the /Config/FemaleFirstnames.txt and returns a list from its content.
    /// </summary>
    /// <returns>A list containing each line of the file.</returns>
    public List<string> GetFemaleFirstnames();

    /// <summary>
    /// Loads the /Config/MaleFirstnames.txt and returns a list from its content.
    /// </summary>
    /// <returns>A list containing each line of the file.</returns>
    public List<string> GetMaleFirstnames();

    /// <summary>
    /// Loads the /Config/Lastnames.txt and returns a list from its content.
    /// </summary>
    /// <returns>A list containing each line of the file.</returns>
    public List<string> GetLastnames();

    /// <summary>
    /// Loads and returns the database connection settings.
    /// </summary>
    /// <returns>Returns the database connection settings.</returns>
    [NoUsage]
    public DatabaseConfig GetDatabaseConfig();

    /// <summary>
    /// Loads and returns the menu configuration.
    /// </summary>
    /// <returns>Returns the menu configuration.</returns>
    public MenuConfig GetMenuConfig();

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
    /// asd.
    /// </summary>
    /// <returns>qwe.</returns>
    public FeaturesConfig GetFeaturesConfig();
}