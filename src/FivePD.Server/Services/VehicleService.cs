using System;
using System.Collections.Concurrent;
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
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IVehicleService" />
[Obfuscate]
public class VehicleService : IVehicleService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IConfigService _configService;
    private readonly IPedService _pedService;
    private readonly DatabaseContext _databaseContext;

    private readonly ConcurrentDictionary<int, FVehicle> _vehicles = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="configService">The <see cref="IConfigService" /> to use.</param>
    /// <param name="pedService">The <see cref="IPedService" /> to use.</param>
    /// <param name="databaseContext">The <see cref="DatabaseContext" /> to use.</param>
    public VehicleService(
        EventHandlerDictionary eventHandlerDictionary,
        IConfigService configService,
        IPedService pedService,
        DatabaseContext databaseContext)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._configService = configService;
        this._pedService = pedService;
        this._databaseContext = databaseContext;
    }

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.Vehicle.Generate] += new Action<string>(this.GenerateVehicle);
    }

    /// <inheritdoc/>
    public FVehicle TryGetVehicle(int networkId)
    {
        var found = this._vehicles.TryGetValue(networkId, out var vehicle);
        if (found)
        {
            return vehicle;
        }

        vehicle = FVehicle.FromNetId(networkId); // TODO: this should not exist, it's an empty instance from the builder (??)
        if (vehicle is null)
        {
            return null;
        }

        this._vehicles[networkId] = vehicle;

        return vehicle;
    }

    private void GenerateVehicle(string serializedFVehicleBuilder)
    {
        Debug.WriteLine($"GenerateVehicle - {serializedFVehicleBuilder}");
        var fVehicleBuilderDto = JsonConvert.DeserializeObject<CommonFVehicleBuilder>(serializedFVehicleBuilder);
        if (fVehicleBuilderDto is null || this._vehicles.ContainsKey(fVehicleBuilderDto.NetworkId))
        {
            return;
        }

        var fVehicle = new FVehicleBuilder(fVehicleBuilderDto)
            .WithItems(this._configService.Items)
            .WithRandomDocuments(this._configService.GeneralConfig.Vehicles.Documents)
            .WithPedHistory(this._pedService.TryGetPed(fVehicleBuilderDto.OwnerNetworkId).GetPedHistory())
            .Build();

        Debug.WriteLine($"Generated data for vehicle with net ID of {fVehicle.NetworkId} - License plate: {fVehicle.LicensePlate}");

        fVehicle.HasGeneratedData.Value = true;
        this._vehicles[fVehicle.NetworkId] = fVehicle;
    }

    [UIEvent(Events.Vehicle.Get)]
    private void GetUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var fVehicle = this.TryGetVehicle(request.Id);
        if (fVehicle is null)
        {
            networkCallbackDelegate.InvokeWithJson(null);
            return;
        }

        networkCallbackDelegate.InvokeWithJson(fVehicle.ToDto(this._pedService.TryGetPed(fVehicle.OwnerNetworkId).GetPedHistory()));
    }

    [UIEvent(Events.Vehicle.GetAllPageable)]
    private void GetAllPageableUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<VehicleGetAllPageableRequest>();
        var pageNumber = request.Page < 1 ? 1 : request.Page;

        var values = this._vehicles.Values.AsQueryable();
        var filters = new Filters<FVehicle>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.LicensePlate),
            vehicle => vehicle.LicensePlate.ToUpper().Contains(request.Filters.LicensePlate.ToUpper()));

        values = filters.Get().Aggregate(values, (current, filter) => current.Where(x => filter.Expression.Compile().Invoke(x)));

        const int pageSize = QueryableExtensions.DefaultPageSize;
        var page = new Page<FVehicleDTO>
        {
            CurrentPage = pageNumber,
            PageSize = pageSize,
            ItemCount = this._vehicles.Count,
            Items = values
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(vehicle => vehicle.ToDto(this._databaseContext.PedHistories.FirstOrDefault(pedHistory => pedHistory.Id == vehicle.PedHistoryId)))
                .ToList(),
        };
        var pageCount = (int)Math.Ceiling((double)page.ItemCount / pageSize);
        page.PageCount = pageCount == 0 ? 1 : pageCount;

        networkCallbackDelegate.InvokeWithJson(page);
    }
}