using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.AssistanceServiceModels;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services;

#pragma warning disable

public class AssistanceServiceConfigService : IOnInit, IAssistanceServiceConfigService
{
    private readonly ILogger _logger;
    private readonly INetworkEventService _networkEventService;
    private readonly IDatabaseContextFactory _dbContextFactory;

    private readonly List<AvailableAssistanceService> _availableServices = new();
    private readonly List<Type> _registrationOrder = new();

    public AssistanceServiceConfigService(IDatabaseContextFactory dbContextFactory, ILogger logger, INetworkEventService networkEventService)
    {
        this._dbContextFactory = dbContextFactory;
        this._logger = logger;
        this._networkEventService = networkEventService;
    }

    void IOnInit.OnInit()
    {
        using var dbScope = this._dbContextFactory.CreateScope();
        var databaseContext = dbScope.Context;

        var configs = databaseContext.AssistanceServiceTypeConfigs.ToList();
        var filteredConfigs = configs.Where(x => !this._availableServices.Any(r => r.TypeName == x.TypeName && r.AssemblyName == x.AssemblyName))
            .ToList();
        foreach (var config in filteredConfigs)
        {
            config.Active = false;
        }

        var stationVehicleConfigs = databaseContext.AssistanceServiceStationTypeConfigs.ToList();
        foreach (var config in stationVehicleConfigs)
        {
            config.ActiveCapacity = 0;
        }

        databaseContext.SaveChanges();
    }

    public IReadOnlyList<AvailableAssistanceService> AvailableServices => this._availableServices.AsReadOnly();

    /// <inheritdoc />
    public void Register<T>(AssistanceServiceOptions options)
        where T : AssistanceService
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var type = typeof(T);
        if (this._availableServices.Any(x => x.Type == type))
        {
            this._logger.Error(
                "Can not register assistance service with type \"{Type}\". An assistance service with the same type has been already registered.", type);
            return;
        }

        if (string.IsNullOrEmpty(options.RequestLabel))
        {
            this._logger.Error("Can not register assistance service with type \"{Type}\". The RequestLabel is null or empty.", type);
            return;
        }

        if (string.IsNullOrEmpty(options.CancelLabel))
        {
            this._logger.Error("Can not register assistance service with type \"{Type}\". The CancelLabel is null or empty.", type);
            return;
        }

        this._logger.Information("Registering an assistance service of type \"{Type}\"", type);

        var typeName = type.Name;
        var assemblyName = type.Assembly.GetName().Name;
        var assistanceServiceVehicleConfig = databaseContext.AssistanceServiceTypeConfigs
            .FirstOrDefault(x => x.TypeName == typeName && x.AssemblyName == assemblyName);
        if (assistanceServiceVehicleConfig is null)
        {
            databaseContext.AssistanceServiceTypeConfigs.Add(new AssistanceServiceTypeConfig
            {
                Active = true,
                TypeName = typeName,
                AssemblyName = assemblyName,
                Enabled = true,
                ManualSpawnLocations = new(),
                SpawnType = "AUTO",
            });
        }
        else
        {
            assistanceServiceVehicleConfig.Active = true;
        }

        databaseContext.SaveChanges();

        this._availableServices.Add(new AvailableAssistanceService
        {
            Type = type,
            Options = options,
            TypeName = typeName,
            AssemblyName = assemblyName,
        });
        this._registrationOrder.Add(type);
    }

    [UIEvent(Events.AssistanceService.GetTypeConfig)]
    private void GetTypeConfigUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var assistanceServiceTypeConfig = databaseContext.AssistanceServiceTypeConfigs.FirstOrDefault(x => x.Id == request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, assistanceServiceTypeConfig);
    }

    [UIEvent(Events.AssistanceService.GetTypeConfigPage)]
    private void GetTypeConfigPageUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<AssistanceServiceGetTypePageRequest>(serializedRequest);
        var filters = new Filters<AssistanceServiceTypeConfig>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.TypeName),
            x => x.TypeName.ToLower().Contains(request.Filters.TypeName.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.AssemblyName),
            x => x.AssemblyName.ToLower().Contains(request.Filters.AssemblyName.ToLower()));
        var page = databaseContext.AssistanceServiceTypeConfigs
            .AsNoTracking()
            .Paginate(request.Page, filters);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.AssistanceService.EditTypeConfig)]
    private void EditTypeConfigUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<EditAssistanceServiceTypeConfig>(serializedRequest);
        var assistanceServiceTypeConfig = databaseContext.AssistanceServiceTypeConfigs.FirstOrDefault(x => x.Id == request.Id);
        if (assistanceServiceTypeConfig is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        assistanceServiceTypeConfig.Enabled = request.Enabled;
        assistanceServiceTypeConfig.SpawnType = request.SpawnType;
        assistanceServiceTypeConfig.ManualSpawnLocations = request.ManualSpawnLocations;

        databaseContext.SaveChanges();

        this._networkEventService.TriggerEventSerialized(player, guid, assistanceServiceTypeConfig);
    }

    [UIEvent(Events.AssistanceService.GetStation)]
    private void GetStationUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var assistanceServiceStation = databaseContext.AssistanceServiceStations
            .Include(x => x.TypeConfigs)
            .FirstOrDefault(x => x.Id == request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, assistanceServiceStation);
    }

    [UIEvent(Events.AssistanceService.GetStationPage)]
    private void GetStationPageUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<AssistanceServiceGetStationPageRequest>(serializedRequest);
        var filters = new Filters<AssistanceServiceStation>();
        var page = databaseContext.AssistanceServiceStations
            .AsNoTracking()
            .Paginate(request.Page, filters);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.AssistanceService.CreateStation)]
    private void CreateStationUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<CreateAssistanceServiceStationRequest>(serializedRequest);
        var assistanceServiceStation = databaseContext.AssistanceServiceStations.FirstOrDefault(x => x.Name == request.Name);
        if (assistanceServiceStation is not null)
        {
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                false,
                "ERROR.ASSISTANCE_SERVICE.STATION.CREATE.TITLE",
                "ERROR.ASSISTANCE_SERVICE.STATION.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        databaseContext.AssistanceServiceStations.Add(new AssistanceServiceStation
        {
            Name = request.Name,
            Position = request.Position,
        });
        databaseContext.SaveChanges();

        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.CREATE",
            "LABEL.ASSISTANCE_SERVICE.STATION.SUCCESS.CREATE");
    }

    [UIEvent(Events.AssistanceService.DeleteStation)]
    private void DeleteStationUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var assistanceServiceStation = databaseContext.AssistanceServiceStations.FirstOrDefault(x => x.Id == request.Id);
        if (assistanceServiceStation is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        databaseContext.AssistanceServiceStations.Remove(assistanceServiceStation);
        databaseContext.SaveChanges();

        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.DELETE",
            "LABEL.ASSISTANCE_SERVICE.STATION.SUCCESS.DELETE");
    }

    [UIEvent(Events.AssistanceService.GetStationTypeConfigsPage)]
    private void GetStationTypeConfigsPageUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<AssistanceServiceGetStationTypeConfigsPageRequest>(serializedRequest);
        var filters = new Filters<AssistanceServiceStationTypeConfig>();
        var page = databaseContext.AssistanceServiceStationTypeConfigs
            .AsNoTracking()
            .Include(x => x.TypeConfig)
            .Paginate(request.Page, filters);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.AssistanceService.GetStationTypeConfig)]
    private void GetStationTypeConfigUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var assistanceServiceStationTypeConfig = databaseContext.AssistanceServiceStationTypeConfigs
            .AsNoTracking()
            .Include(x => x.TypeConfig)
            .FirstOrDefault(x => x.Id == request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, assistanceServiceStationTypeConfig);
    }

    [UIEvent(Events.AssistanceService.AddStationTypeConfig)]
    private void AddStationTypeConfigUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<AddAssistanceServiceStationTypeConfigRequest>(serializedRequest);
        var assistanceServiceStation = databaseContext.AssistanceServiceStations.FirstOrDefault(x => x.Id == request.StationId);
        if (assistanceServiceStation is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var assistanceServiceTypeConfig = databaseContext.AssistanceServiceTypeConfigs.FirstOrDefault(x => x.Id == request.TypeConfigId);
        if (assistanceServiceTypeConfig is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        assistanceServiceStation.TypeConfigs.Add(new AssistanceServiceStationTypeConfig
        {
            TypeConfig = assistanceServiceTypeConfig,
            Capacity = request.Capacity,
            ActiveCapacity = 0
        });
        databaseContext.SaveChanges();

        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.ADD",
            "LABEL.ASSISTANCE_SERVICE.STATION.TYPE_CONFIG.SUCCESS.ADD");
    }

    [UIEvent(Events.AssistanceService.EditStationTypeConfig)]
    private void EditStationTypeConfigUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<EditAssistanceServiceStationTypeConfig>(serializedRequest);
        var assistanceServiceStationTypeConfig = databaseContext.AssistanceServiceStationTypeConfigs.FirstOrDefault(x => x.Id == request.Id);
        if (assistanceServiceStationTypeConfig is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        assistanceServiceStationTypeConfig.Capacity = request.Capacity;
        databaseContext.SaveChanges();

        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.EDIT",
            "LABEL.ASSISTANCE_SERVICE.STATION.TYPE_CONFIG.SUCCESS.ADD");
    }

    [UIEvent(Events.AssistanceService.DeleteStationTypeConfig)]
    private void DeleteStationTypeConfigUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var assistanceServiceStationTypeConfig = databaseContext.AssistanceServiceStationTypeConfigs.FirstOrDefault(x => x.Id == request.Id);
        if (assistanceServiceStationTypeConfig is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        databaseContext.AssistanceServiceStationTypeConfigs.Remove(assistanceServiceStationTypeConfig);
        databaseContext.SaveChanges();

        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.DELETE",
            "LABEL.ASSISTANCE_SERVICE.STATION.TYPE_CONFIG.SUCCESS.DELETE");
    }

    [UIEvent(Events.AssistanceService.GetAvailableStationTypeConfigs)]
    private void GetAvailableStationTypeConfigsUI(Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;

        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var assistanceServiceStation = databaseContext.AssistanceServiceStations
            .Include(x => x.TypeConfigs)
            .ThenInclude(assistanceServiceStationTypeConfig => assistanceServiceStationTypeConfig.TypeConfig)
            .FirstOrDefault(x => x.Id == request.Id);
        if (assistanceServiceStation is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var typeConfigs = databaseContext.AssistanceServiceTypeConfigs
            .AsNoTracking()
            .Where(x => x.Active && x.Enabled && x.SpawnType == "STATION")
            .ToList();

        var filteredTypeConfigs = typeConfigs.Where(x => assistanceServiceStation.TypeConfigs.All(y => y.TypeConfig.Id != x.Id)).ToList();
        this._networkEventService.TriggerEventSerialized(player, guid, filteredTypeConfigs);
    }
}