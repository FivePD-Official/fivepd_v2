namespace FivePD.Server.Models.LoadoutModels.JoinTables;

/// <summary>
/// Many-to-Many joining table for LoadoutConfig and Weapon.
/// </summary>
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
    public LoadoutConfig LoadoutConfig { get; set; }

    /// <summary>
    /// Gets or sets the weapons.
    /// </summary>
    public Weapon Weapon { get; set; }
}