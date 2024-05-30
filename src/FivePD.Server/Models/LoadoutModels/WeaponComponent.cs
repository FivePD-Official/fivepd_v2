using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FivePD.Server.Models.LoadoutModels.JoinTables;

namespace FivePD.Server.Models.LoadoutModels;

/// <summary>
/// Value type for weapon component.
/// </summary>
[Table("fivepd_weapon_components")]
public class WeaponComponent
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
    /// Gets or sets the hash key.
    /// </summary>
    public string HashKey { get; set; }

    /// <summary>
    /// Gets or sets the Components.
    /// </summary>
    public IList<WeaponWeaponComponent> WeaponWeaponComponents { get; set; } = new List<WeaponWeaponComponent>();
}