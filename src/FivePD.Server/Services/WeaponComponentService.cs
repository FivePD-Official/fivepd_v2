using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class WeaponComponentService : IWeaponComponentService
{
    private readonly DatabaseContext _databaseContext;
    private readonly INetworkEventService _networkEventService;

    public WeaponComponentService(DatabaseContext databaseContext, INetworkEventService networkEventService)
    {
        this._databaseContext = databaseContext;
        this._networkEventService = networkEventService;
    }

    [UIEvent(Events.UI.WeaponComponent.Get)]
    private void GetById([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var weaponComponent = this._databaseContext.WeaponComponents.FirstOrDefault(x => x.Id == request.Id);

        if (weaponComponent is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._networkEventService.TriggerEventSerialized(player, guid, weaponComponent);
    }

    [UIEvent(Events.UI.WeaponComponent.GetAllPageable)]
    private void GetAllPageable([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<WeaponComponentsRequest>(serializedRequest);
        var filters = new Filters<WeaponComponent>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Name),
            wc => wc.Name.ToLower().Contains(request.Filters.Name.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.HashKey),
            wc => wc.HashKey.ToLower().Contains(request.Filters.HashKey.ToLower()));
        var page = this._databaseContext.WeaponComponents
            .AsNoTracking()
            .Paginate(request.Page, filters);

        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.WeaponComponent.GetAllSimple)]
    private void GetAllSimple([FromSource] Player player, string guid, string serializedRequest)
    {
        this._networkEventService.TriggerEventSerialized(player, guid, this._databaseContext.WeaponComponents.Select(x => new
        {
            Id = x.Id,
            Name = x.Name,
            HashKey = x.HashKey
        }));
    }

    [UIEvent(Events.UI.WeaponComponent.Create)]
    private void Create([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AddWeaponComponentRequest>(serializedRequest);

        var weaponComponent = new WeaponComponent() { Name = request.Name, HashKey = request.HashKey };

        this._databaseContext.WeaponComponents.Add(weaponComponent);
        this._databaseContext.SaveChanges();

        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.CREATE",
            "LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.SUCCESS.CREATE");
    }

    [UIEvent(Events.UI.WeaponComponent.Edit)]
    private void Edit([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditWeaponComponentRequest>(serializedRequest);
        var weaponComponent = this._databaseContext.WeaponComponents.FirstOrDefault(x => x.Id == request.Id);

        if (weaponComponent is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        weaponComponent.Name = request.Name;
        weaponComponent.HashKey = request.HashKey;

        this._databaseContext.SaveChanges();
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.EDIT",
            "LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.SUCCESS.EDIT");
    }

    [UIEvent(Events.UI.WeaponComponent.Delete)]
    private void Delete([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var weaponComponent = this._databaseContext.WeaponComponents.FirstOrDefault(x => x.Id == request.Id);

        if (weaponComponent is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._databaseContext.WeaponComponents.Remove(weaponComponent);
        this._databaseContext.SaveChanges();
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.DELETE",
            "LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.SUCCESS.DELETE");
    }
}