using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
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
    private readonly ConcurrentDictionary<int, FVehicle> _vehicles = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="configService">The <see cref="IConfigService" /> to use.</param>
    public VehicleService(EventHandlerDictionary eventHandlerDictionary, IConfigService configService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._configService = configService;
    }

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
#pragma warning disable SA1512
        // this._vehicles[2] = new FVehicle(new CommonFVehicle(new CommonFVehicleBuilder(2).WithLicensePlate("ABC-123").WithBrand("abc brand").WithColor("red")));
        // this._vehicles[4] = new FVehicle(new CommonFVehicle(new CommonFVehicleBuilder(4).WithLicensePlate("DEF-456").WithBrand("def brand").WithColor("blue")));
#pragma warning restore SA1512

        this._eventHandlerDictionary[Events.Vehicle.Generate] += new Action<string>(this.GenerateVehicle);
    }

    /// <inheritdoc/>
    public FVehicle TryGetVehicle(int networkId)
    {
        return this._vehicles.TryGetValue(networkId, out var fVehicle) ? fVehicle : null;
    }

    private void GenerateVehicle(string serializedFVehicleBuilder)
    {
        var fVehicleBuilder = JsonConvert.DeserializeObject<CommonFVehicleBuilder>(serializedFVehicleBuilder);
        if (fVehicleBuilder is null || this._vehicles.ContainsKey(fVehicleBuilder.NetworkId))
        {
            return;
        }

        var commonFVehicle = fVehicleBuilder
            .WithItems(this._configService.Items)
            .Build();

        this._vehicles[commonFVehicle.NetworkId] = new FVehicle(commonFVehicle);
    }

    [UIEvent(Events.Vehicle.GetAllPageable)]
    private void GetAllPageable(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<VehicleGetAllPageableRequest>();
        var pageNumber = request.Page < 1 ? 1 : request.Page;

        var values = this._vehicles.Values.AsQueryable();
        var filters = new Filters<FVehicle>();
        filters.Add(!string.IsNullOrEmpty(request.Filters.LicensePlate), vehicle => vehicle.LicensePlate.ToUpper().Contains(request.Filters.LicensePlate.ToUpper()));

        values = filters.Get().Aggregate(values, (current, filter) => current.Where(filter.Expression));

        const int pageSize = QueryableExtensions.DefaultPageSize;
        var page = new Page<FVehicle>
        {
            CurrentPage = pageNumber,
            PageSize = pageSize,
            ItemCount = this._vehicles.Count,
            Items = values.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
        };
        var pageCount = (int)Math.Ceiling((double)page.ItemCount / pageSize);
        page.PageCount = pageCount == 0 ? 1 : pageCount;

        networkCallbackDelegate.InvokeWithJson(page);
    }
}