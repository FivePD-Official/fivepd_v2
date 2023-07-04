using System;
using System.Collections.Concurrent;
using CitizenFX.Core;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Builders;
using FivePD.Gamemode.Common.Models;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IVehicleService" />
public class VehicleService : IVehicleService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IConfigService _configService;
    private readonly ConcurrentDictionary<int, CommonFVehicle> _vehicles = new();

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
    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Vehicle.Generate] += new Action<string>(this.GenerateVehicle);
    }

    /// <inheritdoc/>
    public FVehicle TryGetVehicle(int networkId)
    {
        var commonFVehicle = this._vehicles.TryGetValue(networkId, out var fVehicle) ? fVehicle : null;
        return new FVehicle(commonFVehicle);
    }

    private void GenerateVehicle(string serializedFVehicleBuilder)
    {
        var fVehicleBuilder = JsonConvert.DeserializeObject<CommonFVehicleBuilder>(serializedFVehicleBuilder);
        if (fVehicleBuilder is null || this._vehicles.ContainsKey(fVehicleBuilder.NetworkId))
        {
            return;
        }

        var fVehicle = fVehicleBuilder
            .WithItems(this._configService.Items)
            .Build();

        this._vehicles[fVehicle.NetworkId] = fVehicle;
    }
}