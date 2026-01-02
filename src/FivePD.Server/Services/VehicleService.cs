using System;
using System.Linq;
using CitizenFX.Core;
using Dapper;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using FivePD.Server.Repositories;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc cref="FivePD.Server.Interfaces.IVehicleService" />
[Obfuscate]
public class VehicleService : IVehicleService, IOnInit
{
    private readonly IInternalVehicleStorageService _vehicleStorageService;
    private readonly IPedStorageService _pedStorageService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IConfigService _configService;
    private readonly INetworkEventService _networkEventService;
    private readonly PedDataRepository _pedDataRepository;
    private readonly VehicleDataRepository _vehicleDataRepository;
    private readonly IDatabaseConnectionFactory _dbConnectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleService"/> class.
    /// </summary>
    /// <param name="vehicleStorageService">The <see cref="IVehicleStorageService" /> to use.</param>
    /// <param name="pedService">The <see cref="IPedStorageService" /> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="configService">The <see cref="IConfigService" /> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService" /> to use.</param>
    public VehicleService(
        IVehicleStorageService vehicleStorageService,
        IPedStorageService pedStorageService,
        EventHandlerDictionary eventHandlerDictionary,
        IConfigService configService,
        INetworkEventService networkEventService,
        PedDataRepository pedDataRepository,
        VehicleDataRepository vehicleDataRepository,
        IDatabaseConnectionFactory dbConnectionFactory)
    {
        this._vehicleStorageService = (IInternalVehicleStorageService)vehicleStorageService;
        this._pedStorageService = pedStorageService;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._configService = configService;
        this._networkEventService = networkEventService;
        this._pedDataRepository = pedDataRepository;
        this._vehicleDataRepository = vehicleDataRepository;
        this._dbConnectionFactory = dbConnectionFactory;
    }

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.Vehicle.Generate] += new Action<string>(this.GenerateVehicle);
    }

    private void GenerateVehicle(string serializedFVehicleBuilder)
    {
        var fVehicleBuilderDto = JsonConvert.DeserializeObject<CommonFVehicleBuilder>(serializedFVehicleBuilder);
        if (fVehicleBuilderDto is null || this._vehicleStorageService.InternalVehicles.ContainsKey(fVehicleBuilderDto.NetworkId))
        {
            return;
        }

        var ownerFPed = this._pedStorageService.TryGetPed(fVehicleBuilderDto.OwnerNetworkId);
        var pedData = ownerFPed is null ? null : this._pedDataRepository.FindById(ownerFPed.PedDataId);
        var fVehicleBuilder = new FVehicleBuilder(fVehicleBuilderDto)
            .WithItems(this._configService.Items)
            .WithRandomDocuments(this._configService.GeneralConfig.Vehicles.Documents)
            .WithPedHistory(pedData);

        var vehicleData = this._vehicleDataRepository.Add(new VehicleData
        {
            Items = fVehicleBuilder.Items,
            LicensePlate = fVehicleBuilder.LicensePlate,
            Brand = fVehicleBuilder.Brand,
            Color = fVehicleBuilder.Color,
            PedDataId = pedData?.Id
        });

        foreach (var document in fVehicleBuilder.Documents)
        {
            this._vehicleDataRepository.AddDocument(vehicleData, document);
        }

        var fVehicle = fVehicleBuilder
            .WithVehicleData(vehicleData)
            .Build();

        Debug.WriteLine($"Generated data for vehicle with net ID of {fVehicleBuilder.NetworkId} - License plate: {fVehicleBuilder.LicensePlate}");

        fVehicle.HasGeneratedData.Value = true;
        this._vehicleStorageService.InternalVehicles[fVehicle.NetworkId] = fVehicle;
    }

    [UIEvent(Events.Vehicle.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var vehicleData =
            this._vehicleDataRepository.FindByIdWithDynamicRelated(
                request.Id,
                new VehicleDataRepository.VehicleDataLoadOptions { IncludeDocuments = true, IncludePedData = true });
        this._networkEventService.TriggerEventSerialized(player, guid, vehicleData);
    }

    [UIEvent(Events.Vehicle.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<VehicleHistoryGetAllRequest>(serializedRequest);
        var page = this._vehicleDataRepository.Paginate(
            request.Page,
            b => b
                .If(!string.IsNullOrEmpty(request.Filters.LicensePlate), x => x.WhereContains("LicensePlate", request.Filters.LicensePlate))
                .If(!string.IsNullOrEmpty(request.Filters.Color), x => x.WhereContains("Color", request.Filters.Color))
                .If(!string.IsNullOrEmpty(request.Filters.Brand), x => x.WhereContains("Brand", request.Filters.Brand)));
        
        var builderResult = SqlBuilder
            .From($"{TableNames.VehicleData} vd")
            .Select("vd.*", typeof(VehicleData))
            .Select("pd.*", typeof(PedData))
            .LeftJoin(TableNames.PedData, "pd", "vd.PedDataId = pd.Id")
            .WhereIn("vd.Id", page.Items.Select(x => x.Id))
            .Build();
        var mappedItems = this._vehicleDataRepository.QueryMultiMap<VehicleData, int>(builderResult, (entity, rowReader) =>
        {
            var pedData = rowReader.Read<PedData>(true);
            if (pedData is not null && entity.PedData is null)
            {
                entity.PedData = pedData;
            }

            return entity;
        }, x => x.Id);
        page.Items = mappedItems;
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }
}