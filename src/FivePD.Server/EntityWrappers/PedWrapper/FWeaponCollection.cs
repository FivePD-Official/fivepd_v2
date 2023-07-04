using System.Threading.Tasks;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.DTO;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Server;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Server.EntityWrappers.PedWrapper;

#pragma warning disable

/// <summary>
/// Represents the weapons of a <see cref="FPed"/>.
/// </summary>
public class FWeaponCollection
{
    private readonly FPed _fPed;
    private readonly IEventService _eventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FWeaponCollection"/> class.
    /// </summary>
    /// <param name="fPed">The <see cref="FPed"/> to use.</param>
    public FWeaponCollection(FPed fPed)
    {
        this._fPed = fPed;
        this._eventService = ServiceLocator.GetService<IEventService>();
    }

    public void Give(FWeaponHash hash, int ammoCount, bool equipNow, bool isAmmoLoaded)
    {
        this.TriggerWeaponCollectionEvent(WeaponCollectionDTO.Events.Give, new WeaponCollectionDTO.Give
        {
            Hash = hash,
            AmmoCount = ammoCount,
            EquipNow = equipNow,
            IsAmmoLoaded = isAmmoLoaded,
        });
    }

    public async Task<bool> HasWeapon(FWeaponHash weaponHash)
    {
        var value = await this.TriggerWeaponCollectionEventAsync(
            WeaponCollectionDTO.Events.HasWeapon,
            new WeaponCollectionDTO.HasWeapon
            {
                Hash = weaponHash,
            });

        return bool.Parse(value);
    }

    public async Task<bool> IsWeaponValid(FWeaponHash hash)
    {
        var value = await this.TriggerWeaponCollectionEventAsync(
            WeaponCollectionDTO.Events.IsWeaponValid,
            new WeaponCollectionDTO.IsWeaponValid
            {
                Hash = hash,
            });

        return bool.Parse(value);
    }

    public async Task<FWeapon> Current()
    {
        var value = await this.TriggerWeaponCollectionEventAsync(WeaponCollectionDTO.Events.Current, string.Empty);
        return JsonConvert.DeserializeObject<FWeapon>(value);
    }

    public void Remove(FWeapon weapon)
    {
        this.TriggerWeaponCollectionEvent(WeaponCollectionDTO.Events.Remove, new WeaponCollectionDTO.Remove
        {
            Hash = weapon.Hash,
        });
    }

    public void Remove(FWeaponHash weaponHash)
    {
        this.TriggerWeaponCollectionEvent(WeaponCollectionDTO.Events.Remove, new WeaponCollectionDTO.Remove
        {
            Hash = weaponHash,
        });
    }

    public void RemoveAll()
    {
        this.TriggerWeaponCollectionEvent(WeaponCollectionDTO.Events.RemoveAll, string.Empty);
    }

    public void Drop()
    {
        this.TriggerWeaponCollectionEvent(WeaponCollectionDTO.Events.Drop, string.Empty);
    }

    public void Select(FWeapon weapon)
    {
        this.TriggerWeaponCollectionEvent(WeaponCollectionDTO.Events.Select, new WeaponCollectionDTO.Select
        {
            Hash = weapon.Hash,
        });
    }

    public void Select(FWeaponHash hash)
    {
        this.TriggerWeaponCollectionEvent(WeaponCollectionDTO.Events.Select, new WeaponCollectionDTO.Select
        {
            Hash = hash,
        });
    }
    
    public void AddComponentToWeapon(FWeaponHash weaponHash, uint componentHash)
    {
        this.TriggerWeaponCollectionEvent(WeaponCollectionDTO.Events.AddComponentToWeapon, new WeaponCollectionDTO.AddComponentToWeapon
        {
            WeaponHash = (uint)weaponHash,
            ComponentHash = componentHash,
        });
    }

    private void TriggerWeaponCollectionEvent<T>(string subEvent, T args)
    {
        this._eventService.TriggerEventWithoutCallback(
            this._fPed.Owner,
            Events.Ped.WeaponCollection,
            JsonConvert.SerializeObject(new WeaponCollectionDTO.WeaponCollectionRequest
            {
                NetworkId = this._fPed.GetFPed.NetworkId,
                SubEvent = subEvent,
                Args = JsonConvert.SerializeObject(args),
            }));
    }

    private async Task<string> TriggerWeaponCollectionEventAsync<T>(string subEvent, T args)
    {
        var value = await this._eventService.TriggerEventAsync(
            this._fPed.Owner,
            Events.Ped.WeaponCollectionWithReturn,
            JsonConvert.SerializeObject(new WeaponCollectionDTO.WeaponCollectionRequest
            {
                NetworkId = this._fPed.GetFPed.NetworkId,
                SubEvent = subEvent,
                Args = JsonConvert.SerializeObject(args),
            }));

        return value;
    }

    public sealed class FWeapon
    {
        public int Ammo { get; init; }

        public int AmmoInClip { get; init; }

        // TODO ? public AmmoType AmmoType => throw null;

        public bool CanUseOnParachute { get; init; }

        // TODO ? public WeaponComponentCollection Components => throw null;

        public int DefaultClipSize { get; init; }

        public string DisplayName { get; init; }

        public FWeaponHash Hash { get; init; }

        public string LocalizedName { get; init; }

        public int MaxAmmo { get; init; }

        public int MaxAmmoInClip { get; init; }

        // TODO ?
        // public bool InfiniteAmmo
        // {
        //     set { }
        // }

        // TODO ?
        // public bool InfiniteAmmoClip
        // {
        //     set { }
        // }
    }
}