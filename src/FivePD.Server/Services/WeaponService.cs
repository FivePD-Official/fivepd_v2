using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models.Requests;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class WeaponService : IWeaponService
{
    private readonly DatabaseContext _databaseContext;
    private readonly INetworkEventService _networkEventService;

    public WeaponService(DatabaseContext databaseContext, INetworkEventService networkEventService)
    {
        this._databaseContext = databaseContext;
        this._networkEventService = networkEventService;
    }

    [UIEvent(Events.UI.Weapon.Get)]
    private void GetById([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var weapon = this._databaseContext.Weapons
            .Include(x => x.WeaponWeaponComponents)
            .Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                Model = x.Model,
                WeaponComponentIds = x.WeaponWeaponComponents.Select(wwc => wwc.WeaponComponentId),
                WeaponComponentHashes = x.WeaponWeaponComponents.Select(wwc => wwc.WeaponComponent.HashKey)
            })
            .FirstOrDefault(x => x.Id == request.Id);

        if (weapon is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._networkEventService.TriggerEventSerialized(player, guid, weapon);
    }

    [UIEvent(Events.UI.Weapon.GetAllPageable)]
    private void GetAllPageable([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<WeaponsRequest>(serializedRequest);
        var filters = new Filters<Weapon>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Name),
            weapon => weapon.Name.ToLower().Contains(request.Filters.Name.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Model),
            weapon => weapon.Model.ToLower().Contains(request.Filters.Model.ToLower()));
        var page = this._databaseContext.Weapons
            .Include(x => x.WeaponWeaponComponents)
            .AsNoTracking()
            .Paginate(request.Page, filters);

        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.Weapon.GetAllSimple)]
    private void GetAllSimple([FromSource] Player player, string guid, string serializedRequest)
    {
        this._networkEventService.TriggerEventSerialized(player, guid, this._databaseContext.Weapons.Select(x => new
        {
            Id = x.Id,
            Name = x.Name,
        }));
    }

    [UIEvent(Events.UI.Weapon.Create)]
    private void Create([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AddWeaponRequest>(serializedRequest);

        var weapon = new Weapon() { Name = request.Name, Model = request.Model };

        foreach (var weaponComponentHashKey in request.WeaponComponentHashes)
        {
            var component = this._databaseContext.WeaponComponents.FirstOrDefault(x => x.HashKey == weaponComponentHashKey);

            if (component is not null)
            {
                weapon.AddComponent(component);
            }
        }

        this._databaseContext.Weapons.Add(weapon);
        this._databaseContext.SaveChanges();

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.CREATE", "LABEL.LOADOUT_CONFIG.WEAPON.SUCCESS.CREATE");
    }

    [UIEvent(Events.UI.Weapon.Edit)]
    private void Edit([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditWeaponRequest>(serializedRequest);
        var weapon = this._databaseContext.Weapons.Include(x => x.WeaponWeaponComponents).FirstOrDefault(x => x.Id == request.Id);

        if (weapon is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        weapon.Name = request.Name;
        weapon.Model = request.Model;

        var existingWeaponComponents = weapon.WeaponWeaponComponents.Select(x => x.WeaponComponent.HashKey).ToList();
        var weaponComponentsToAdd = request.WeaponComponentHashes.Except(existingWeaponComponents).ToList();
        var weaponComponentsToRemove = weapon.WeaponWeaponComponents.Where(x => !request.WeaponComponentHashes.Contains(x.WeaponComponent.HashKey)).ToList();

        foreach (var weaponComponentHash in weaponComponentsToAdd)
        {
            var weaponComponent = this._databaseContext.WeaponComponents.FirstOrDefault(x => x.HashKey == weaponComponentHash);

            if (weaponComponent is null)
            {
                this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
                return;
            }

            weapon.AddComponent(weaponComponent);
        }

        foreach (var weaponWeaponComponent in weaponComponentsToRemove)
        {
            var weaponComponent = this._databaseContext.WeaponComponents.Find(weaponWeaponComponent.WeaponComponentId);

            if (weaponComponent is null)
            {
                this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
                return;
            }

            weapon.RemoveComponent(weaponWeaponComponent);
        }

        this._databaseContext.SaveChanges();
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.EDIT", "LABEL.LOADOUT_CONFIG.WEAPON.SUCCESS.EDIT");
    }

    [UIEvent(Events.UI.Weapon.Delete)]
    private void Delete([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var weapon = this._databaseContext.Weapons.FirstOrDefault(x => x.Id == request.Id);

        if (weapon is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._databaseContext.Weapons.Remove(weapon);
        this._databaseContext.SaveChanges();
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.DELETE", "LABEL.LOADOUT_CONFIG.WEAPON.SUCCESS.DELETE");
    }
}