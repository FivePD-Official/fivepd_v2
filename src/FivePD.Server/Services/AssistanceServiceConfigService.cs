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
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services;

#pragma warning disable

public class AssistanceServiceConfigService : IOnInit, IAssistanceServiceConfigService
{
    private readonly DatabaseContext _databaseContext;
    private readonly ILogger _logger;
    private readonly INetworkEventService _networkEventService;

    private readonly List<AvailableAssistanceService> _availableServices = new();
    private readonly List<Type> _registrationOrder = new();

    public AssistanceServiceConfigService(DatabaseContext databaseContext, ILogger logger, INetworkEventService networkEventService)
    {
        this._databaseContext = databaseContext;
        this._logger = logger;
        this._networkEventService = networkEventService;
    }

    void IOnInit.OnInit()
    {
        var configs = this._databaseContext.AssistanceServiceTypeConfigs.ToList();
        var filteredConfigs = configs.Where(x => !this._availableServices.Any(r => r.TypeName == x.TypeName && r.AssemblyName == x.AssemblyName))
            .ToList();
        foreach (var config in filteredConfigs)
        {
            config.Active = false;
        }

        var stationVehicleConfigs = this._databaseContext.AssistanceServiceStationVehicleConfigs.ToList();
        foreach (var config in stationVehicleConfigs)
        {
            config.ActiveCapacity = 0;
        }

        this._databaseContext.SaveChanges();
    }

    public IReadOnlyList<AvailableAssistanceService> AvailableServices => this._availableServices.AsReadOnly();

    /// <inheritdoc />
    public void Register<T>(AssistanceServiceOptions options)
        where T : AssistanceService
    {
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
        var assistanceServiceVehicleConfig = this._databaseContext.AssistanceServiceTypeConfigs
            .FirstOrDefault(x => x.TypeName == typeName && x.AssemblyName == assemblyName);
        if (assistanceServiceVehicleConfig is null)
        {
            this._databaseContext.AssistanceServiceTypeConfigs.Add(new AssistanceServiceTypeConfig
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

        this._databaseContext.SaveChanges();

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
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var assistanceServiceTypeConfig = this._databaseContext.AssistanceServiceTypeConfigs.FirstOrDefault(x => x.Id == request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, assistanceServiceTypeConfig);
    }

    [UIEvent(Events.AssistanceService.GetTypeConfigPage)]
    private void GetTypeConfigPageUI(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AssistanceServiceGetTypePageRequest>(serializedRequest);
        var filters = new Filters<AssistanceServiceTypeConfig>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.TypeName),
            x => x.TypeName.ToLower().Contains(request.Filters.TypeName.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.AssemblyName),
            x => x.AssemblyName.ToLower().Contains(request.Filters.AssemblyName.ToLower()));
        var page = this._databaseContext.AssistanceServiceTypeConfigs
            .AsNoTracking()
            .Paginate(request.Page, filters);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }
    
    [UIEvent(Events.AssistanceService.EditTypeConfig)]
    private void EditTypeConfigUI(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditAssistanceServiceTypeConfig>(serializedRequest);
        var assistanceServiceTypeConfig = this._databaseContext.AssistanceServiceTypeConfigs.FirstOrDefault(x => x.Id == request.Id);
        if (assistanceServiceTypeConfig is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }
        
        assistanceServiceTypeConfig.Enabled = request.Enabled;
        assistanceServiceTypeConfig.SpawnType = request.SpawnType;
        assistanceServiceTypeConfig.ManualSpawnLocations = request.ManualSpawnLocations;
        
        this._databaseContext.SaveChanges();
        
        this._networkEventService.TriggerEventSerialized(player, guid, assistanceServiceTypeConfig);
    }
}