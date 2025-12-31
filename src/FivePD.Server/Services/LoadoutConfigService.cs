using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using Dapper;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Models.Requests;
using FivePD.Server.Repositories;
using FivePD.Server.UIModels;
using FivePD.Server.UIModels.MenuModels;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class LoadoutConfigService : ILoadoutConfigService, IOnInit
{
    private readonly ILogger _logger;
    private readonly LoadoutConfigRepository _loadoutConfigRepository;
    private readonly WeaponRepository _weaponRepository;
    private readonly IDapperDatabaseConnectionFactory _dbConnectionFactory;
    private readonly IMenuService _menuService;
    private readonly PlayerList _playerList;
    private readonly INetworkEventService _networkEventService;

    public LoadoutConfigService(
        ILogger logger,
        LoadoutConfigRepository loadoutConfigRepository,
        WeaponRepository weaponRepository,
        IDapperDatabaseConnectionFactory dbConnectionFactory,
        IMenuService menuService,
        PlayerList playerList,
        INetworkEventService networkEventService)
    {
        this._logger = logger;
        this._loadoutConfigRepository = loadoutConfigRepository;
        this._weaponRepository = weaponRepository;
        this._dbConnectionFactory = dbConnectionFactory;
        this._menuService = menuService;
        this._playerList = playerList;
        this._networkEventService = networkEventService;
    }

    void IOnInit.OnInit()
    {
        // var a = this._loadoutConfigRepository.GetAllWithDynamicRelated(new LoadoutConfigRepository.LoadoutConfigLoadOptions { IncludeWeapons = false });
        // Debug.WriteLine(JsonConvert.SerializeObject(a, Formatting.Indented));
    }

    public List<LoadoutConfig> GetAll()
    {
        return this._loadoutConfigRepository.GetAllWithDynamicRelated(new LoadoutConfigRepository.LoadoutConfigLoadOptions { IncludeWeapons = true });
    }

    [UIEvent(Events.UI.LoadoutConfig.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var loadoutConfig =
            this._loadoutConfigRepository.FindByIdWithDynamicRelated(
                request.Id,
                new LoadoutConfigRepository.LoadoutConfigLoadOptions { IncludeWeapons = false });
        if (loadoutConfig is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._networkEventService.TriggerEventSerialized(player, guid, new
        {
            Id = loadoutConfig.Id,
            Name = loadoutConfig.Name,
            WeaponIds = loadoutConfig.LoadoutWeapons.Select(wid => wid.WeaponId),
        });
    }

    [UIEvent(Events.UI.LoadoutConfig.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<LoadoutConfigsRequest>(serializedRequest);
        var page = this._loadoutConfigRepository.Paginate(
            request.Page,
            b => b
                .If(!string.IsNullOrEmpty(request.Filters.Name), x => x.WhereContains("Name", request.Filters.Name)));
        if (!page.Items.Any())
        {
            this._networkEventService.TriggerEventSerialized(player, guid, page);
            return;
        }

        var builderResult = SqlBuilder
            .From($"{TableNames.LoadoutConfigs} lc")
            .Select("lc.Id as LoadoutConfigId, COUNT(lw.LoadoutConfigId) as LoadoutWeaponCount")
            .GroupBy("lc.Id")
            .LeftJoin(TableNames.LoadoutWeapon, "lw", "lw.LoadoutConfigId = lc.Id")
            .WhereIn("lc.Id", page.Items.Select(d => d.Id))
            .Build();
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var count = connection.Query<LoadoutConfigLoadoutWeaponCountDto>(builderResult.Sql, builderResult.Parameters).ToDictionary(c => c.LoadoutConfigId);
        page.Items = page.Items
            .Select(item => count.TryGetValue(item.Id, out var x) ? new LoadoutConfigResponse(item, x.LoadoutWeaponCount) : null)
            .ToList();
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.LoadoutConfig.Create)]
    private void CreateLoadoutConfigUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AddLoadoutConfigRequest>(serializedRequest);

        var loadoutConfig = new LoadoutConfig { Name = request.Name };
        this._loadoutConfigRepository.Add(loadoutConfig);

        foreach (var weaponId in request.WeaponIds)
        {
            var weapon = this._weaponRepository.FindById(weaponId);
            if (weapon is not null)
            {
                this._loadoutConfigRepository.AddWeapon(loadoutConfig, weapon);
            }
        }

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.CREATE", "LABEL.LOADOUT_CONFIG.SUCCESS.CREATE");
        this.SendNewMenuItems();
    }

    [UIEvent(Events.UI.LoadoutConfig.Edit)]
    private void EditLoadoutConfigUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditLoadoutConfigRequest>(serializedRequest);
        var loadout = this._loadoutConfigRepository.FindByIdWithDynamicRelated(
            request.Id,
            new LoadoutConfigRepository.LoadoutConfigLoadOptions { IncludeWeapons = false });
        if (loadout is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._loadoutConfigRepository.Update(loadout, x => { x.Name = request.Name; });

        var existingWeapons = loadout.LoadoutWeapons.Select(x => x.WeaponId).ToList();
        var weaponsToAdd = request.WeaponIds.Except(existingWeapons).ToList();
        var weaponsToRemove = loadout.LoadoutWeapons.Where(x => !request.WeaponIds.Contains(x.WeaponId)).ToList();

        foreach (var weaponId in weaponsToAdd)
        {
            var weapon = this._weaponRepository.FindById(weaponId);
            if (weapon is null)
            {
                this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
                return;
            }

            this._loadoutConfigRepository.AddWeapon(loadout, weapon);
        }

        foreach (var loadoutWeapon in weaponsToRemove)
        {
            var weapon = this._weaponRepository.FindById(loadoutWeapon.WeaponId);
            if (weapon is null)
            {
                this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
                return;
            }

            this._loadoutConfigRepository.RemoveWeapon(loadout, weapon);
        }

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.EDIT", "LABEL.LOADOUT_CONFIG.SUCCESS.EDIT");
        this.SendNewMenuItems();
    }

    [UIEvent(Events.UI.LoadoutConfig.Delete)]
    private void DeleteLoadoutConfigUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var loadout = this._loadoutConfigRepository.FindById(request.Id);
        if (loadout is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._loadoutConfigRepository.Delete(request.Id);
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.DELETE", "LABEL.LOADOUT_CONFIG.SUCCESS.DELETE");
        this.SendNewMenuItems();
    }

    private void SendNewMenuItems()
    {
        var newMenuOptions = this.GetAll().Select(loadout => new MenuListOption(loadout.Name)).ToList();
        foreach (var player in this._playerList)
        {
            this._menuService.UpdateMenuListItem<LoadoutSelectorMenu, List<MenuListOption>>(
                FPlayer.FromPlayer(player),
                menu => menu.LoadoutItem,
                item => item.Options,
                newMenuOptions
            );
        }
    }

#pragma warning disable
    private class LoadoutConfigLoadoutWeaponCountDto
    {
        public int LoadoutConfigId { get; set; }

        public int LoadoutWeaponCount { get; set; }
    }
#pragma warning restore
}