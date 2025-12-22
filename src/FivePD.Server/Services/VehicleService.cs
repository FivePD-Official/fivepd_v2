using System;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
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
    private readonly IDatabaseContextFactory _dbContextFactory;
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleService"/> class.
    /// </summary>
    /// <param name="vehicleStorageService">The <see cref="IVehicleStorageService" /> to use.</param>
    /// <param name="pedService">The <see cref="IPedStorageService" /> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="configService">The <see cref="IConfigService" /> to use.</param>
    /// <param name="dbContextFactory">The <see cref="IDatabaseContextFactory" /> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService" /> to use.</param>
    public VehicleService(
        IVehicleStorageService vehicleStorageService,
        IPedStorageService pedStorageService,
        EventHandlerDictionary eventHandlerDictionary,
        IConfigService configService,
        IDatabaseContextFactory dbContextFactory,
        INetworkEventService networkEventService)
    {
        this._vehicleStorageService = (IInternalVehicleStorageService)vehicleStorageService;
        this._pedStorageService = pedStorageService;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._configService = configService;
        this._dbContextFactory = dbContextFactory;
        this._networkEventService = networkEventService;
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

        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var ownerFPed = this._pedStorageService.TryGetPed(fVehicleBuilderDto.OwnerNetworkId);
        var pedData = ownerFPed is null ? null : databaseContext.PedData.FirstOrDefault(x => x.Id == ownerFPed.PedDataId);
        var fVehicleBuilder = new FVehicleBuilder(fVehicleBuilderDto)
            .WithItems(this._configService.Items)
            .WithRandomDocuments(this._configService.GeneralConfig.Vehicles.Documents)
            .WithPedHistory(pedData);

        var entry = databaseContext.VehicleData.Add(new VehicleData
        {
            Items = fVehicleBuilder.Items,
            LicensePlate = fVehicleBuilder.LicensePlate,
            Documents = fVehicleBuilder.Documents,
            Brand = fVehicleBuilder.Brand,
            Color = fVehicleBuilder.Color,
            PedDataId = pedData?.Id
        });

        if (pedData is not null)
        {
            var existingVehicleData = databaseContext.VehicleData.FirstOrDefault(x => x.PedDataId == pedData.Id);
            if (existingVehicleData is not null)
            {
                existingVehicleData.PedDataId = null;
                databaseContext.SaveChanges();
            }
        }

        databaseContext.SaveChanges();

        if (pedData is not null)
        {
            pedData.VehicleDataId = entry.Entity.Id;
            databaseContext.SaveChanges();
        }

        var fVehicle = fVehicleBuilder
            .WithVehicleData(entry.Entity)
            .Build();

        Debug.WriteLine($"Generated data for vehicle with net ID of {fVehicleBuilder.NetworkId} - License plate: {fVehicleBuilder.LicensePlate}");

        fVehicle.HasGeneratedData.Value = true;
        this._vehicleStorageService.InternalVehicles[fVehicle.NetworkId] = fVehicle;
    }

    [UIEvent(Events.Vehicle.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var vehicleData = databaseContext.VehicleData
            .AsNoTracking()
            .Include(x => x.PedData)
            .Include(x => x.Documents)
            .FirstOrDefault(x => x.Id == request.Id);

        this._networkEventService.TriggerEventSerialized(player, guid, vehicleData);
    }

    [UIEvent(Events.Vehicle.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<VehicleHistoryGetAllRequest>(serializedRequest);

        var filters = new Filters<VehicleData>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.LicensePlate),
            pedHistory => pedHistory.LicensePlate.ToLower().Contains(request.Filters.LicensePlate.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Color),
            pedHistory => pedHistory.Color.ToLower().Contains(request.Filters.Color.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Brand),
            pedHistory => pedHistory.Brand.ToLower().Contains(request.Filters.Brand.ToLower()));

        var page = databaseContext.VehicleData
            .AsNoTracking()
            .Include(x => x.PedData)
            .Paginate(request.Page, filters);

        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }
}