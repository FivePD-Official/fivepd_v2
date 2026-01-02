using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Models.Requests;
using FivePD.Server.Repositories;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class WeaponComponentService : IWeaponComponentService
{
    private readonly WeaponComponentRepository _weaponComponentRepository;
    private readonly INetworkEventService _networkEventService;

    public WeaponComponentService(WeaponComponentRepository weaponComponentRepository, INetworkEventService networkEventService)
    {
        this._weaponComponentRepository = weaponComponentRepository;
        this._networkEventService = networkEventService;
    }

    [UIEvent(Events.UI.WeaponComponent.Get)]
    private void GetById([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var weaponComponent = this._weaponComponentRepository.FindById(request.Id);
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
        var page = this._weaponComponentRepository.Paginate(
            request.Page,
            b => b
                .If(!string.IsNullOrEmpty(request.Filters.Name), x => x.WhereContains("Name", request.Filters.Name))
                .If(!string.IsNullOrEmpty(request.Filters.HashKey), x => x.WhereContains("HashKey", request.Filters.HashKey)));
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.WeaponComponent.GetAllSimple)]
    private void GetAllSimple([FromSource] Player player, string guid, string serializedRequest)
    {
        var weaponComponents = this._weaponComponentRepository.GetAll();
        this._networkEventService.TriggerEventSerialized(player, guid, weaponComponents.Select(x => new
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

        var weaponComponent = new WeaponComponent { Name = request.Name, HashKey = request.HashKey };
        this._weaponComponentRepository.Add(weaponComponent);

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
        var weaponComponent = this._weaponComponentRepository.FindById(request.Id);

        if (weaponComponent is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._weaponComponentRepository.Update(weaponComponent, x =>
        {
            x.Name = request.Name;
            x.HashKey = request.HashKey;
        });
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
        var weaponComponent = this._weaponComponentRepository.FindById(request.Id);
        if (weaponComponent is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._weaponComponentRepository.Delete(weaponComponent.Id);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.DELETE",
            "LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.SUCCESS.DELETE");
    }
}