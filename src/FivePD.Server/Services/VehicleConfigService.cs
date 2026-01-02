using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.Repositories;
using FivePD.Server.UIModels;
using FivePD.Server.UIModels.MenuModels;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services;

/// <summary>
/// Service for managing vehicle configurations.
/// </summary>
[Obfuscate]
public class VehicleConfigService : IVehicleConfigService, IOnInit
{
    private readonly ILogger _logger;
    private readonly VehicleConfigRepository _vehicleConfigRepository;
    private readonly IMenuService _menuService;
    private readonly PlayerList _playerList;
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleConfigService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="vehicleConfigRepository">The <see cref="VehicleConfigRepository"/> to use.</param>
    /// <param name="menuService">The menu service.</param>
    /// <param name="playerList">The player list.</param>
    /// <param name="eventHandlerDictionary">The event handler dictionary.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public VehicleConfigService(
        ILogger logger,
        VehicleConfigRepository vehicleConfigRepository,
        IMenuService menuService,
        PlayerList playerList,
        EventHandlerDictionary eventHandlerDictionary,
        INetworkEventService networkEventService)
    {
        this._logger = logger;
        this._vehicleConfigRepository = vehicleConfigRepository;
        this._menuService = menuService;
        this._playerList = playerList;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._networkEventService = networkEventService;
    }

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.VehicleConfig.RequestPoliceVehicleModels] += new Action<Player, string>(this.OnRequestPoliceVehicleModels);
    }

    /// <inheritdoc />
    public List<VehicleConfig<Dictionary<string, object>>> GetAll(VehicleConfigType type)
    {
        return this.GetAll<Dictionary<string, object>>(type);
    }

    /// <inheritdoc />
    public List<VehicleConfig<TOptions>> GetAll<TOptions>(VehicleConfigType type)
    {
        return this._vehicleConfigRepository.GetAll<TOptions>(b => b.Where("Type = @Type", new { Type = type })).ToList();
    }

    [UIEvent(Events.UI.VehicleConfig.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var vehicleConfig = this._vehicleConfigRepository.FindById(request.Id);
        if (vehicleConfig is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._networkEventService.TriggerEventSerialized(player, guid, vehicleConfig);
    }

    [UIEvent(Events.UI.VehicleConfig.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<VehicleConfigsRequest>(serializedRequest);
        var page = this._vehicleConfigRepository.Paginate(
            request.Page,
            b => b
                .If(!string.IsNullOrEmpty(request.Filters.Name), x => x.WhereContains("Name", request.Filters.Name))
                .If(!string.IsNullOrEmpty(request.Filters.Model), x => x.WhereContains("Model", request.Filters.Model))
                .If(request.Filters.Type != -1, x => x.Where("Type = @Type", request.Filters.Type)));
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.VehicleConfig.Create)]
    private void CreateVehicleConfigUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AddVehicleConfigRequest>(serializedRequest);
        this._vehicleConfigRepository.Add(new VehicleConfig<Dictionary<string, object>>
        {
            Type = request.Type,
            Name = request.Name,
            Model = request.Model,
            Livery = request.Livery,
            Extras = request.Extras,
            Options = request.Options,
        });
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.CREATE", "LABEL.VEHICLE_CONFIG.SUCCESS.CREATE");
        this.SendNewMenuItems();
        this.SendPoliceVehicleConfigUpdatesToAllClients();
    }

    [UIEvent(Events.UI.VehicleConfig.Edit)]
    private void EditVehicleConfigUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditVehicleConfigRequest>(serializedRequest);
        var vehicleConfig = this._vehicleConfigRepository.FindById(request.Id);
        if (vehicleConfig is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._vehicleConfigRepository.Update(vehicleConfig, x =>
        {
            x.Name = request.Name;
            x.Model = request.Model;
            x.Livery = request.Livery;
            x.Extras = request.Extras;
            x.Options = JsonConvert.SerializeObject(request.Options);
        });

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.EDIT", "LABEL.VEHICLE_CONFIG.SUCCESS.EDIT");
        this.SendNewMenuItems();
        this.SendPoliceVehicleConfigUpdatesToAllClients();
    }

    [UIEvent(Events.UI.VehicleConfig.Delete)]
    private void DeleteVehicleConfigUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        if (!this._vehicleConfigRepository.Exists(request.Id))
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._vehicleConfigRepository.Delete(request.Id);
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.DELETE", "LABEL.VEHICLE_CONFIG.SUCCESS.DELETE");
        this.SendNewMenuItems();
        this.SendPoliceVehicleConfigUpdatesToAllClients();
    }

    private void SendNewMenuItems()
    {
        var newMenuOptions = this.GetAll(VehicleConfigType.Police).Select(vehicle => new MenuListOption(vehicle.Name)).ToList();
        foreach (var player in this._playerList)
        {
            this._menuService.UpdateMenuListItem<VehicleSelectorMenu, List<MenuListOption>>(
                FPlayer.FromPlayer(player),
                menu => menu.VehicleSpawnerItem,
                item => item.Options,
                newMenuOptions);
        }
    }

    private void OnRequestPoliceVehicleModels([FromSource] Player player, string guid)
    {
        var policeVehicleModels = this.GetAllPoliceVehicleModelNames();
        this._networkEventService.TriggerEventSerialized(player, guid, policeVehicleModels);
    }

    private void SendPoliceVehicleConfigUpdatesToAllClients()
    {
        var policeVehicleModels = this.GetAllPoliceVehicleModelNames();
        var serializedPoliceVehicleModels = JsonConvert.SerializeObject(policeVehicleModels);

        foreach (var player in this._playerList)
        {
            player.TriggerEvent(Events.VehicleConfig.UpdatePoliceVehicleModels, serializedPoliceVehicleModels);
        }
    }

    private List<string> GetAllPoliceVehicleModelNames()
    {
        return this._vehicleConfigRepository
            .GetAll(b => b.Where("Type = @Type", new { Type = VehicleConfigType.Police }))
            .Select(x => x.Model)
            .ToList();
    }
}