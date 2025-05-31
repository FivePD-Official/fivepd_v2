using FivePD.Common.Attributes;
using FivePD.Common.Enums;

namespace FivePD.Common.DTO;

#pragma warning disable

[Obfuscate]
internal sealed record WeaponCollectionDTO
{
    public record struct WeaponCollectionRequest
    {
        public int NetworkId { get; set; }
    
        public string SubEvent { get; set; }
    
        public string Args { get; set; }
    }

    public record struct Give
    {
        public FWeaponHash Hash { get; set; }

        public int AmmoCount { get; set; }

        public bool EquipNow { get; set; }

        public bool IsAmmoLoaded { get; set; }
    }

    public record struct HasWeapon
    {
        public FWeaponHash Hash { get; set; }
    }

    public record struct IsWeaponValid
    {
        public FWeaponHash Hash { get; set; }
    }

    public record struct Remove
    {
        public FWeaponHash Hash { get; set; }
    }

    public record struct Select
    {
        public FWeaponHash Hash { get; set; }
    }
    
    public record struct CurrentFWeapon
    {
        public int Ammo { get; init; }

        public int AmmoInClip { get; init; }

        // TODO public AmmoType AmmoType => throw null;

        public bool CanUseOnParachute { get; init; }

        // TODO public WeaponComponentCollection Components => throw null;

        public int DefaultClipSize  { get; init; }

        public string DisplayName  { get; init; }

        public FWeaponHash Hash  { get; init; }

        public string LocalizedName  { get; init; }

        public int MaxAmmo  { get; init; }

        public int MaxAmmoInClip  { get; init; }
    }
    
    public record struct AddComponentToWeapon
    {
        public uint WeaponHash { get; set; }

        public uint ComponentHash { get; set; }
    }

    public sealed record Events
    {
        public const string Give = "Give";

        public const string HasWeapon = "HasWeapon";

        public const string IsWeaponValid = "IsWeaponValid";

        public const string Remove = "Remove";

        public const string RemoveAll = "RemoveAll";

        public const string Select = "Select";

        public const string Drop = "Drop";

        public const string Current = "Current";

        public const string AddComponentToWeapon = "AddComponentToWeapon";
    }
}