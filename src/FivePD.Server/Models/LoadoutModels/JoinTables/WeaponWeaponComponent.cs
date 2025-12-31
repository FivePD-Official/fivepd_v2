namespace FivePD.Server.Models.LoadoutModels.JoinTables;

/// <summary>
/// Many-to-Many joining table for LoadoutConfig and Weapon.
/// </summary>
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
    public Weapon Weapon { get; set; }

    /// <summary>
    /// Gets or sets the weapons.
    /// </summary>
    public WeaponComponent WeaponComponent { get; set; }
}