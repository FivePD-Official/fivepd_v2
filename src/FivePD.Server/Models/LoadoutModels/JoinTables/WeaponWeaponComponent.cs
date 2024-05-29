using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models.LoadoutModels.JoinTables;

/// <summary>
/// Many-to-Many joining table for LoadoutConfig and Weapon.
/// </summary>
[Table("fivepd_weapon_weaponComponent")]
public class WeaponWeaponComponent
{
    /// <summary>
    /// Gets or sets the LoadoutConfigId.
    /// </summary>
    public int WeaponId { get; set; }

    /// <summary>
    /// Gets or sets the WeaponId.
    /// </summary>
    public int WeaponComponentId { get; set; }

    /// <summary>
    /// Gets or sets the weapons.
    /// </summary>
    public virtual Weapon Weapon { get; set; }

    /// <summary>
    /// Gets or sets the weapons.
    /// </summary>
    public virtual WeaponComponent WeaponComponent { get; set; }
}