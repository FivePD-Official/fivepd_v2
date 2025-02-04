using System;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Extensions;
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
    private readonly DatabaseContext _databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleService"/> class.
    /// </summary>
    /// <param name="vehicleStorageService">The <see cref="IVehicleStorageService" /> to use.</param>
    /// <param name="pedService">The <see cref="IPedStorageService" /> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="configService">The <see cref="IConfigService" /> to use.</param>
    /// <param name="databaseContext">The <see cref="DatabaseContext" /> to use.</param>
    public VehicleService(
        IVehicleStorageService vehicleStorageService,
        IPedStorageService pedStorageService,
        EventHandlerDictionary eventHandlerDictionary,
        IConfigService configService,
        DatabaseContext databaseContext)
    {
        this._vehicleStorageService = (IInternalVehicleStorageService)vehicleStorageService;
        this._pedStorageService = pedStorageService;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._configService = configService;
        this._databaseContext = databaseContext;
    }

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.Vehicle.Generate] += new Action<string>(this.GenerateVehicle);
    }

    private void GenerateVehicle(string serializedFVehicleBuilder)
    {
        Debug.WriteLine($"GenerateVehicle - {serializedFVehicleBuilder}");
        var fVehicleBuilderDto = JsonConvert.DeserializeObject<CommonFVehicleBuilder>(serializedFVehicleBuilder);
        if (fVehicleBuilderDto is null || this._vehicleStorageService.InternalVehicles.ContainsKey(fVehicleBuilderDto.NetworkId))
        {
            return;
        }

        var ownerFPed = this._pedStorageService.TryGetPed(fVehicleBuilderDto.OwnerNetworkId);
        var pedData = ownerFPed is null ? null : this._databaseContext.PedData.FirstOrDefault(x => x.Id == ownerFPed.PedDataId);
        var fVehicleBuilder = new FVehicleBuilder(fVehicleBuilderDto)
            .WithItems(this._configService.Items)
            .WithRandomDocuments(this._configService.GeneralConfig.Vehicles.Documents)
            .WithPedHistory(pedData);

        var entry = this._databaseContext.VehicleData.Add(new VehicleData
        {
            Items = fVehicleBuilder.Items,
            LicensePlate = fVehicleBuilder.LicensePlate,
            Documents = fVehicleBuilder.Documents,
            Brand = fVehicleBuilder.Brand,
            Color = fVehicleBuilder.Color,
            PedDataId = pedData?.Id ?? 0
        });
        this._databaseContext.SaveChanges();

        if (pedData is not null)
        {
            pedData.VehicleDataId = entry.Entity.Id;
            this._databaseContext.SaveChanges();
        }

        var fVehicle = fVehicleBuilder
            .WithVehicleData(entry.Entity)
            .Build();

        Debug.WriteLine($"Generated data for vehicle with net ID of {fVehicleBuilder.NetworkId} - License plate: {fVehicleBuilder.LicensePlate}");

        fVehicle.HasGeneratedData.Value = true;
        this._vehicleStorageService.InternalVehicles[fVehicle.NetworkId] = fVehicle;
    }

    [UIEvent(Events.Vehicle.Get)]
    private void GetUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var vehicleHistory = this._databaseContext.VehicleData
            .AsNoTracking()
            .Include(x => x.PedData)
            .Include(x => x.Documents)
            .FirstOrDefault(x => x.Id == request.Id);

        networkCallbackDelegate.InvokeWithJson(vehicleHistory);
    }

    [UIEvent(Events.Vehicle.GetAllPageable)]
    private void GetAllPageableUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<VehicleHistoryGetAllRequest>();

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

        var page = this._databaseContext.VehicleData
            .AsNoTracking()
            .Include(x => x.PedData)
            .Paginate(request.Page, filters);

        networkCallbackDelegate.InvokeWithJson(page);
    }
}