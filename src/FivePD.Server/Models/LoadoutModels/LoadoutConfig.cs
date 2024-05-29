using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FivePD.Server.Models.LoadoutModels.JoinTables;

namespace FivePD.Server.Models.LoadoutModels;

/// <summary>
/// Used to get/set weapon config.
/// </summary>
[Table("fivepd_loadout_configs")]
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
    public IList<LoadoutWeapon> LoadoutWeapons { get; set; } = new List<LoadoutWeapon>();

    /// <summary>
    /// Adds a weapon to a loadout.
    /// </summary>
    /// <param name="weapon">weapon.</param>
    internal void AddWeapon(Weapon weapon)
    {
        var loadoutWeapon = new LoadoutWeapon { LoadoutConfig = this, Weapon = weapon };
        this.LoadoutWeapons.Add(loadoutWeapon);
    }

    /// <summary>
    /// Removes a weapon from a loadout.
    /// </summary>
    /// <param name="loadoutWeapon">weapon.</param>
    internal void RemoveWeapon(LoadoutWeapon loadoutWeapon)
    {
        this.LoadoutWeapons.Remove(loadoutWeapon);
    }
}