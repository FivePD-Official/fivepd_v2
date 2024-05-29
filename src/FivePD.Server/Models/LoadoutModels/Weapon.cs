using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FivePD.Server.Models.LoadoutModels.JoinTables;

namespace FivePD.Server.Models.LoadoutModels;

/// <summary>
/// Used to get/set weapon config.
/// </summary>
[Table("fivepd_weapons")]
public class Weapon
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
    /// Gets or sets the model.
    /// </summary>
    /// <remarks>The model name can sometimes be known as the WeaponId. Such as: WEAPON_PISTOL.</remarks>
    public string Model { get; set; }

    /// <summary>
    /// Gets or sets the Components.
    /// </summary>
    public IList<WeaponWeaponComponent> WeaponWeaponComponents { get; set; } = new List<WeaponWeaponComponent>();

    /// <summary>
    /// Gets or sets the LoadoutWeapon.
    /// </summary>
    public IList<LoadoutWeapon> LoadoutWeapons { get; set; } = new List<LoadoutWeapon>();

    /// <summary>
    /// Adds a weapon component to a weapon.
    /// </summary>
    /// <param name="weaponComponent">weapon.</param>
    internal void AddComponent(WeaponComponent weaponComponent)
    {
        var weaponWeaponComponent = new WeaponWeaponComponent { Weapon = this, WeaponComponent = weaponComponent };
        this.WeaponWeaponComponents.Add(weaponWeaponComponent);
    }

    /// <summary>
    /// Removes a weapon component from a weapon.
    /// </summary>
    /// <param name="weaponWeaponComponent">weapon.</param>
    internal void RemoveComponent(WeaponWeaponComponent weaponWeaponComponent)
    {
        this.WeaponWeaponComponents.Remove(weaponWeaponComponent);
    }
}