using FivePD.Common.Attributes;
using FivePD.Server.Models.LoadoutModels;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

[Obfuscate]
internal class WeaponResponse : Weapon
{
    public WeaponResponse(Weapon weapon, int weaponWeaponComponentCount)
    {
        this.Id = weapon.Id;
        this.Name = weapon.Name;
        this.Model = weapon.Model;
        this.WeaponWeaponComponentCount = weaponWeaponComponentCount;
    }

    public int WeaponWeaponComponentCount { get; private set; }
}