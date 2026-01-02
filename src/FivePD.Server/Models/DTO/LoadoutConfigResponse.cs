using FivePD.Common.Attributes;
using FivePD.Server.Models.LoadoutModels;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

[Obfuscate]
internal class LoadoutConfigResponse : LoadoutConfig
{
    public LoadoutConfigResponse(LoadoutConfig loadoutConfig, int loadoutWeaponCount)
    {
        this.Id = loadoutConfig.Id;
        this.Name = loadoutConfig.Name;
        this.LoadoutWeaponCount = loadoutWeaponCount;
    }

    public int LoadoutWeaponCount { get; private set; }
}