using System.Collections.Generic;
using FivePD.Common.ConfigModels;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides utility methods for the FivePD resource itself.
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// Gets the "items.json" file's content.
    /// </summary>
    /// <returns>Returns all items from the config file.</returns>
    public List<Item> Items { get; }

    /// <summary>
    /// Gets the "FemaleFirstnames.txt" file's content.
    /// </summary>
    /// <returns>A list containing each line of the file.</returns>
    public List<string> FemaleFirstnames { get; }

    /// <summary>
    /// Gets the "MaleFirstnames.txt" file's content.
    /// </summary>
    /// <returns>A list containing each line of the file.</returns>
    public List<string> MaleFirstnames { get; }

    /// <summary>
    /// Gets the "Lastnames.txt" file's content.
    /// </summary>
    /// <returns>A list containing each line of the file.</returns>
    public List<string> Lastnames { get; }

    /// <summary>
    /// Gets the "menu.json" file's content.
    /// </summary>
    /// <returns>Returns a deserialized a list of <see cref="Loadout"/> objects.</returns>
    public List<Loadout> LoadoutsConfig { get; }

    /// <summary>
    /// Gets the "general-settings.json" file's content.
    /// </summary>
    public GeneralConfig GeneralConfig { get; }
}