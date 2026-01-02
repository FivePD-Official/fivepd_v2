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
using FivePD.Server.Models.Requests;
using FivePD.Server.Repositories;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc cref="IAssistanceServiceConfigService" />
public class AssistanceServiceConfigService : IOnInit, IAssistanceServiceConfigService
{
    private readonly ILogger _logger;
    private readonly INetworkEventService _networkEventService;
    private readonly AssistanceServiceTypeConfigRepository _assistanceServiceTypeConfigRepository;
    private readonly AssistanceServiceStationRepository _assistanceServiceStationRepository;
    private readonly AssistanceServiceStationTypeConfigRepository _assistanceServiceStationTypeConfigRepository;
    private readonly IDatabaseConnectionFactory _dbConnectionFactory;

    private readonly List<AvailableAssistanceService> _availableServices = new();
    private readonly List<Type> _registrationOrder = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AssistanceServiceConfigService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    /// <param name="assistanceServiceTypeConfigRepository">The <see cref="AssistanceServiceTypeConfigRepository"/> to use.</param>
    /// <param name="assistanceServiceStationRepository">The <see cref="AssistanceServiceStationRepository"/> to use.</param>
    /// <param name="assistanceServiceStationTypeConfigRepository">The <see cref="AssistanceServiceStationTypeConfigRepository"/> to use.</param>
    /// <param name="dbConnectionFactory">The <see cref="IDatabaseConnectionFactory"/> to use.</param>
    public AssistanceServiceConfigService(
        ILogger logger,
        INetworkEventService networkEventService,
        AssistanceServiceTypeConfigRepository assistanceServiceTypeConfigRepository,
        AssistanceServiceStationRepository assistanceServiceStationRepository,
        AssistanceServiceStationTypeConfigRepository assistanceServiceStationTypeConfigRepository,
        IDatabaseConnectionFactory dbConnectionFactory)
    {
        this._logger = logger;
        this._networkEventService = networkEventService;
        this._assistanceServiceTypeConfigRepository = assistanceServiceTypeConfigRepository;
        this._assistanceServiceStationRepository = assistanceServiceStationRepository;
        this._assistanceServiceStationTypeConfigRepository = assistanceServiceStationTypeConfigRepository;
        this._dbConnectionFactory = dbConnectionFactory;
    }

    /// <inheritdoc />
    public IReadOnlyList<AvailableAssistanceService> AvailableServices => this._availableServices.AsReadOnly();

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        var configs = this._assistanceServiceTypeConfigRepository.GetAll().ToList();
        var filteredConfigs = configs
            .Where(x => !this._availableServices.Any(r => r.TypeName == x.TypeName && r.AssemblyName == x.AssemblyName))
            .ToList();
        foreach (var config in filteredConfigs)
        {
            this._assistanceServiceTypeConfigRepository.Update(config, x => { x.Active = false; });
        }

        this._assistanceServiceStationTypeConfigRepository.ResetActiveCapacity();
    }

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
        var assistanceServiceVehicleConfig = this._assistanceServiceTypeConfigRepository.GetByTypeNameAndAssembly(typeName, assemblyName);
        if (assistanceServiceVehicleConfig is null)
        {
            this._assistanceServiceTypeConfigRepository.Add(new AssistanceServiceTypeConfig
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
            this._assistanceServiceTypeConfigRepository.Update(assistanceServiceVehicleConfig, x => { x.Active = true; });
        }

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
        var assistanceServiceTypeConfig = this._assistanceServiceTypeConfigRepository.FindById(request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, assistanceServiceTypeConfig);
    }

    [UIEvent(Events.AssistanceService.GetTypeConfigPage)]
    private void GetTypeConfigPageUI(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AssistanceServiceGetTypePageRequest>(serializedRequest);
        var page = this._assistanceServiceTypeConfigRepository.Paginate(
            request.Page,
            b => b
                .If(!string.IsNullOrEmpty(request.Filters.TypeName), x => x.Where("TypeName LIKE @TypeName", new { TypeName = request.Filters.TypeName }))
                .If(
                    !string.IsNullOrEmpty(request.Filters.AssemblyName),
                    x => x.Where("AssemblyName LIKE @AssemblyName", new { AssemblyName = request.Filters.AssemblyName })));
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.AssistanceService.EditTypeConfig)]
    private void EditTypeConfigUI(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditAssistanceServiceTypeConfig>(serializedRequest);
        var assistanceServiceTypeConfig = this._assistanceServiceTypeConfigRepository.FindById(request.Id);
        if (assistanceServiceTypeConfig is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._assistanceServiceTypeConfigRepository.Update(
            assistanceServiceTypeConfig,
            x =>
            {
                x.Enabled = request.Enabled;
                x.SpawnType = request.SpawnType;
                x.ManualSpawnLocations = request.ManualSpawnLocations;
            });
        this._networkEventService.TriggerEventSerialized(player, guid, assistanceServiceTypeConfig);
    }

    [UIEvent(Events.AssistanceService.GetStation)]
    private void GetStationUI(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var assistanceServiceStation = this._assistanceServiceStationRepository.FindByIdWithDynamicRelated(
            request.Id,
            new AssistanceServiceStationRepository.AssistanceServiceStationLoadOptions { IncludeVehicleTypeConfigs = false });
        this._networkEventService.TriggerEventSerialized(player, guid, assistanceServiceStation);
    }

    [UIEvent(Events.AssistanceService.GetStationPage)]
    private void GetStationPageUI(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AssistanceServiceGetStationPageRequest>(serializedRequest);
        var page = this._assistanceServiceStationRepository.Paginate(request.Page);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.AssistanceService.CreateStation)]
    private void CreateStationUI(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CreateAssistanceServiceStationRequest>(serializedRequest);
        var exists = this._assistanceServiceStationRepository.Exists(b => b.Where("Name = @Name", new { request.Name }));
        if (exists)
        {
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                false,
                "ERROR.ASSISTANCE_SERVICE.STATION.CREATE.TITLE",
                "ERROR.ASSISTANCE_SERVICE.STATION.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        this._assistanceServiceStationRepository.Add(
            new AssistanceServiceStation
            {
                Name = request.Name,
                Position = request.Position,
            });

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
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        if (!this._assistanceServiceStationRepository.Exists(request.Id))
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._assistanceServiceStationRepository.Delete(request.Id);
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
        var request = JsonConvert.DeserializeObject<AssistanceServiceGetStationTypeConfigsPageRequest>(serializedRequest);
        var page = this._assistanceServiceStationTypeConfigRepository.Paginate(
            request.Page,
            b => b.Where("AssistanceServiceStationId = @AssistanceServiceStationId", new { request.Filters.AssistanceServiceStationId }));
        if (!page.Items.Any())
        {
            this._networkEventService.TriggerEventSerialized(player, guid, page);
            return;
        }

        var typeConfigs = this._assistanceServiceTypeConfigRepository
            .GetAll(b => b.WhereIn("Id", page.Items.Select(x => x.TypeConfigId)))
            .ToList();
        page.Items = page.Items
            .Select(item =>
            {
                item.TypeConfig = typeConfigs.FirstOrDefault(x => x.Id == item.TypeConfigId);
                return item;
            })
            .ToList();
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.AssistanceService.GetStationTypeConfig)]
    private void GetStationTypeConfigUI(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var assistanceServiceStationTypeConfig = this._assistanceServiceStationTypeConfigRepository.FindByIdWithDynamicRelated(
            request.Id,
            new AssistanceServiceStationTypeConfigRepository.AssistanceServiceStationTypeConfigLoadOptions { IncludeTypeConfig = true });
        this._networkEventService.TriggerEventSerialized(player, guid, assistanceServiceStationTypeConfig);
    }

    [UIEvent(Events.AssistanceService.AddStationTypeConfig)]
    private void AddStationTypeConfigUI(Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<AddAssistanceServiceStationTypeConfigRequest>(serializedRequest);
        if (!this._assistanceServiceStationRepository.Exists(request.StationId))
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        if (!this._assistanceServiceTypeConfigRepository.Exists(request.TypeConfigId))
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._assistanceServiceStationTypeConfigRepository.Add(
            new AssistanceServiceStationTypeConfig
            {
                AssistanceServiceStationId = request.StationId,
                TypeConfigId = request.TypeConfigId,
                Capacity = request.Capacity,
                ActiveCapacity = 0,
            });

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
        var request = JsonConvert.DeserializeObject<EditAssistanceServiceStationTypeConfig>(serializedRequest);
        var assistanceServiceStationTypeConfig = this._assistanceServiceStationTypeConfigRepository.FindById(request.Id);
        if (assistanceServiceStationTypeConfig is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._assistanceServiceStationTypeConfigRepository.Update(
            assistanceServiceStationTypeConfig,
            x => { x.Capacity = request.Capacity; });

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
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        if (!this._assistanceServiceStationTypeConfigRepository.Exists(request.Id))
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._assistanceServiceStationTypeConfigRepository.Delete(request.Id);

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
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var assistanceServiceStation = this._assistanceServiceStationRepository.FindByIdWithDynamicRelated(
            request.Id,
            new AssistanceServiceStationRepository.AssistanceServiceStationLoadOptions { IncludeVehicleTypeConfigs = true });
        if (assistanceServiceStation is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var typeConfigs = this._assistanceServiceTypeConfigRepository
            .GetAll(b => b
                .Where("Active = @Active", new { Active = true })
                .Where("Enabled = @Enabled", new { Enabled = true })
                .Where("SpawnType = @SpawnType", new { SpawnType = "STATION" }))
            .ToList();

        var filteredTypeConfigs = typeConfigs.Where(x => assistanceServiceStation.TypeConfigs.All(y => y.TypeConfigId != x.Id)).ToList();
        this._networkEventService.TriggerEventSerialized(player, guid, filteredTypeConfigs);
    }
}