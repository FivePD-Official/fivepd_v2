using System.Linq;
using CitizenFX.Core;
using Dapper;
using FivePD.Common;
using FivePD.Common.Models.Requests;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Models.Requests;
using FivePD.Server.Repositories;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class WeaponService : IWeaponService
{
    private readonly WeaponRepository _weaponRepository;
    private readonly WeaponComponentRepository _weaponComponentRepository;
    private readonly IDapperDatabaseConnectionFactory _dbConnectionFactory;
    private readonly INetworkEventService _networkEventService;

    public WeaponService(
        WeaponRepository weaponRepository,
        WeaponComponentRepository weaponComponentRepository,
        IDapperDatabaseConnectionFactory dbConnectionFactory,
        INetworkEventService networkEventService)
    {
        this._weaponRepository = weaponRepository;
        this._weaponComponentRepository = weaponComponentRepository;
        this._dbConnectionFactory = dbConnectionFactory;
        this._networkEventService = networkEventService;
    }

    [UIEvent(Events.UI.Weapon.Get)]
    private void GetById([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var weapon = this._weaponRepository
            .FindByIdWithDynamicRelated(request.Id, new WeaponRepository.WeaponLoadOptions { IncludeWeaponComponents = true });
        if (weapon is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._networkEventService.TriggerEventSerialized(
            player,
            guid,
            new
            {
                Id = weapon.Id,
                Name = weapon.Name,
                Model = weapon.Model,
                WeaponComponentIds = weapon.WeaponWeaponComponents.Select(wwc => wwc.WeaponComponentId),
                WeaponComponentHashes = weapon.WeaponWeaponComponents.Select(wwc => wwc.WeaponComponent.HashKey)
            });
    }

    [UIEvent(Events.UI.Weapon.GetAllPageable)]
    private void GetAllPageable([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<WeaponsRequest>(serializedRequest);
        var page = this._weaponRepository.Paginate(
            request.Page,
            b => b
                .If(!string.IsNullOrEmpty(request.Filters.Name), x => x.WhereContains("Name", request.Filters.Name))
                .If(!string.IsNullOrEmpty(request.Filters.Model), x => x.WhereContains("Model", request.Filters.Model)));
        if (!page.Items.Any())
        {
            this._networkEventService.TriggerEventSerialized(player, guid, page);
            return;
        }

        var builderResult = SqlBuilder
            .From($"{TableNames.Weapons} lw")
            .Select("lw.Id as WeaponId, COUNT(wwc.WeaponId) as WeaponWeaponComponentCount")
            .GroupBy("lw.Id")
            .LeftJoin(TableNames.WeaponWeaponComponent, "wwc", "wwc.WeaponId = lw.Id")
            .WhereIn("lw.Id", page.Items.Select(d => d.Id))
            .Build();
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var count = connection.Query<WeaponWeaponComponentCountDto>(builderResult.Sql, builderResult.Parameters).ToDictionary(c => c.WeaponId);
        page.Items = page.Items
            .Select(item => count.TryGetValue(item.Id, out var x) ? new WeaponResponse(item, x.WeaponWeaponComponentCount) : null)
            .ToList();
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.Weapon.GetAllSimple)]
    private void GetAllSimple([FromSource] Player player, string guid, string serializedRequest)
    {
        var weapons = this._weaponRepository
            .GetAll()
            .Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
            });
        this._networkEventService.TriggerEventSerialized(player, guid, weapons);
    }

    [UIEvent(Events.UI.Weapon.Create)]
    private void Create([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AddWeaponRequest>(serializedRequest);

        var weapon = new Weapon { Name = request.Name, Model = request.Model };
        this._weaponRepository.Add(weapon);

        foreach (var weaponComponentHashKey in request.WeaponComponentHashes)
        {
            var component = this._weaponComponentRepository.FindByHashKey(weaponComponentHashKey);
            if (component is not null)
            {
                this._weaponRepository.AttachComponent(weapon, component);
            }
        }

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.CREATE", "LABEL.LOADOUT_CONFIG.WEAPON.SUCCESS.CREATE");
    }

    [UIEvent(Events.UI.Weapon.Edit)]
    private void Edit([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditWeaponRequest>(serializedRequest);
        var weapon = this._weaponRepository.FindByIdWithDynamicRelated(
            request.Id,
            new WeaponRepository.WeaponLoadOptions { IncludeWeaponComponents = true });
        if (weapon is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._weaponRepository.Update(weapon, x =>
        {
            x.Name = request.Name;
            x.Model = request.Model;
        });

        var existingWeaponComponents = weapon.WeaponWeaponComponents.Select(x => x.WeaponComponent.HashKey).ToList();
        var weaponComponentsToAdd = request.WeaponComponentHashes.Except(existingWeaponComponents).ToList();
        var weaponComponentsToRemove = weapon.WeaponWeaponComponents.Where(x => !request.WeaponComponentHashes.Contains(x.WeaponComponent.HashKey)).ToList();

        foreach (var weaponComponentHash in weaponComponentsToAdd)
        {
            var weaponComponent = this._weaponComponentRepository.FindByHashKey(weaponComponentHash);
            if (weaponComponent is null)
            {
                this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
                return;
            }

            this._weaponRepository.AttachComponent(weapon, weaponComponent);
        }

        foreach (var weaponWeaponComponent in weaponComponentsToRemove)
        {
            var weaponComponent = this._weaponComponentRepository.FindById(weaponWeaponComponent.WeaponComponentId);
            if (weaponComponent is null)
            {
                this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
                return;
            }

            this._weaponRepository.RemoveComponent(weapon, weaponComponent);
        }

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.EDIT", "LABEL.LOADOUT_CONFIG.WEAPON.SUCCESS.EDIT");
    }

    [UIEvent(Events.UI.Weapon.Delete)]
    private void Delete([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var weapon = this._weaponRepository.FindById(request.Id);
        if (weapon is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._weaponRepository.Delete(request.Id);
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.DELETE", "LABEL.LOADOUT_CONFIG.WEAPON.SUCCESS.DELETE");
    }


#pragma warning disable
    private class WeaponWeaponComponentCountDto
    {
        public int WeaponId { get; set; }

        public int WeaponWeaponComponentCount { get; set; }
    }
#pragma warning restore
}