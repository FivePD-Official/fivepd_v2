using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models.LoadoutModels.JoinTables;

/// <summary>
/// Many-to-Many joining table for LoadoutConfig and Weapon.
/// </summary>
[Table("fivepd_loadout_weapon")]
public class LoadoutWeapon
{
    /// <summary>
    /// Gets or sets the LoadoutConfigId.
    /// </summary>
    public int LoadoutConfigId { get; set; }

    /// <summary>
    /// Gets or sets the WeaponId.
    /// </summary>
    public int WeaponId { get; set; }

    /// <summary>
    /// Gets or sets the weapons.
    /// </summary>
    public virtual LoadoutConfig LoadoutConfig { get; set; }

    /// <summary>
    /// Gets or sets the weapons.
    /// </summary>
    public virtual Weapon Weapon { get; set; }
}