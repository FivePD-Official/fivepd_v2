using System.Collections.Generic;
using FivePD.Server.Models.LoadoutModels.JoinTables;

namespace FivePD.Server.Models.LoadoutModels;

/// <summary>
/// Used to get/set weapon config.
/// </summary>
public class LoadoutConfig
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the LoadoutWeapon.
    /// </summary>
    public List<LoadoutWeapon> LoadoutWeapons { get; set; }
}