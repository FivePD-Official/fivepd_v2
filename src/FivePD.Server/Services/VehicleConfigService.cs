using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using FivePD.Server.UIModels;
using FivePD.Server.UIModels.MenuModels;
using Microsoft.EntityFrameworkCore;
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
    private readonly DatabaseContext _databaseContext;
    private readonly IMenuService _menuService;
    private readonly PlayerList _playerList;
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleConfigService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="databaseContext">The database context.</param>
    /// <param name="menuService">The menu service.</param>
    /// <param name="playerList">The player list.</param>
    /// <param name="eventHandlerDictionary">The event handler dictionary.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public VehicleConfigService(
        ILogger logger,
        DatabaseContext databaseContext,
        IMenuService menuService,
        PlayerList playerList,
        EventHandlerDictionary eventHandlerDictionary,
        INetworkEventService networkEventService)
    {
        this._logger = logger;
        this._databaseContext = databaseContext;
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
        return this._databaseContext.VehicleConfigs.Where(x => x.Type == type).ToList();
    }

    /// <inheritdoc />
    public List<VehicleConfig<TOptions>> GetAll<TOptions>(VehicleConfigType type)
    {
        var vehicleConfigs = this._databaseContext.VehicleConfigs.Where(x => x.Type == type).ToList();

        try
        {
            return vehicleConfigs.Select(vehicleConfig => new VehicleConfig<TOptions>
                {
                    Id = vehicleConfig.Id,
                    Name = vehicleConfig.Name,
                    Type = vehicleConfig.Type,
                    Livery = vehicleConfig.Livery,
                    Model = vehicleConfig.Model,
                    Extras = vehicleConfig.Extras,
                    Options = JsonConvert.DeserializeObject<TOptions>(JsonConvert.SerializeObject(vehicleConfig.Options)),
                })
                .ToList();
        }
        catch (Exception e)
        {
            this._logger.Error("Could not map a vehicle config's Options field to the given type. Exception: {Exception}", e);
        }

        return new List<VehicleConfig<TOptions>>();
    }

    [UIEvent(Events.UI.VehicleConfig.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var vehicleConfig = this._databaseContext.VehicleConfigs.FirstOrDefault(x => x.Id == request.Id);
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
        var filters = new Filters<VehicleConfig<Dictionary<string, object>>>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Name),
            vehicleConfig => vehicleConfig.Name.ToLower().Contains(request.Filters.Name.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Model),
            vehicleConfig => vehicleConfig.Model.ToLower().Contains(request.Filters.Model.ToLower()));
        filters.Add(request.Filters.Type != -1, vehicleConfig => vehicleConfig.Type == (VehicleConfigType)request.Filters.Type);
        var page = this._databaseContext.VehicleConfigs
            .AsNoTracking()
            .Paginate(request.Page, filters);

        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.VehicleConfig.Create)]
    private void CreateVehicleConfigUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AddVehicleConfigRequest>(serializedRequest);
        this._databaseContext.VehicleConfigs.Add(new VehicleConfig<Dictionary<string, object>>
        {
            Type = request.Type,
            Name = request.Name,
            Model = request.Model,
            Livery = request.Livery,
            Extras = request.Extras,
            Options = request.Options,
        });
        this._databaseContext.SaveChanges();
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "UI.LABEL.SUCCESS.CREATE", "UI.LABEL.VEHICLE_CONFIG.SUCCESS.CREATE");
        this.SendNewMenuItems();
        this.SendPoliceVehicleConfigUpdatesToAllClients();
    }

    [UIEvent(Events.UI.VehicleConfig.Edit)]
    private void EditVehicleConfigUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditVehicleConfigRequest>(serializedRequest);
        var vehicleConfig = this._databaseContext.VehicleConfigs.FirstOrDefault(x => x.Id == request.Id);
        if (vehicleConfig is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        vehicleConfig.Name = request.Name;
        vehicleConfig.Model = request.Model;
        vehicleConfig.Livery = request.Livery;
        vehicleConfig.Extras = request.Extras;
        vehicleConfig.Options = request.Options;

        this._databaseContext.SaveChanges();
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "UI.LABEL.SUCCESS.EDIT", "UI.LABEL.VEHICLE_CONFIG.SUCCESS.EDIT");
        this.SendNewMenuItems();
        this.SendPoliceVehicleConfigUpdatesToAllClients();
    }

    [UIEvent(Events.UI.VehicleConfig.Delete)]
    private void DeleteVehicleConfigUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var entityToRemove = this._databaseContext.VehicleConfigs.FirstOrDefault(x => x.Id == request.Id);
        if (entityToRemove is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._databaseContext.VehicleConfigs.Remove(entityToRemove);
        this._databaseContext.SaveChanges();
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "UI.LABEL.SUCCESS.DELETE", "UI.LABEL.VEHICLE_CONFIG.SUCCESS.DELETE");
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
        return this._databaseContext.VehicleConfigs
            .AsNoTracking()
            .Where(x => x.Type == VehicleConfigType.Police)
            .Select(x => x.Model)
            .ToList();
    }
}