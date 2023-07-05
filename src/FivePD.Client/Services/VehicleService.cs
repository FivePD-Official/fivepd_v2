using System;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.DTO;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable // TODO remove

/// <inheritdoc cref="FivePD.Client.Interfaces.IVehicleService" />
internal class VehicleService : IVehicleService, IServiceWithEvents
{
    private readonly IPedService _pedService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleService"/> class.
    /// </summary>
    /// <param name="pedService">The <see cref="IPedService"/> to use.</param>
    public VehicleService(IPedService pedService, EventHandlerDictionary eventHandlerDictionary)
    {
        this._pedService = pedService;
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Vehicle.GetProperty] += new Action<string, int, string>(this.GetPedProperty);
        this._eventHandlerDictionary[Events.Vehicle.SetProperty] += new Action<int, string, string>(this.SetPedProperty);
        this._eventHandlerDictionary[Events.Vehicle.InvokeMethod] += new Action<string>(this.InvokeMethod);
    }

    /// <inheritdoc />
    public void GenerateVehicle(Vehicle vehicle)
    {
        // TODO: Is the Occupants contains the Driver as well?
        this._pedService.GeneratePed(vehicle.Driver);
        foreach (var ped in vehicle.Occupants)
        {
            this._pedService.GeneratePed(ped);
        }

        var fVehicleBuilder = new CommonFVehicleBuilder(vehicle.NetworkId)
            .WithOwner(vehicle.Driver.NetworkId)
            .WithPassengers(vehicle.Occupants.Select(ped => ped.NetworkId).ToList())
            .WithColor(vehicle.GetColorName())
            .WithBrand(vehicle.DisplayName);

        BaseScript.TriggerServerEvent(Events.Vehicle.Generate, JsonConvert.SerializeObject(fVehicleBuilder));
    }
    
    private async void GetPedProperty(string eventCallbackId, int networkId, string property)
    {
        var vehicle = await this.FindVehicle(networkId);
        if (vehicle is null)
        {
            BaseScript.TriggerServerEvent(Events.Generic.Callback, eventCallbackId, string.Empty);
            return;
        }

        object value = property switch
        {
            _ => string.Empty,
        };

        BaseScript.TriggerServerEvent(Events.Generic.Callback, eventCallbackId, value.ToString());
    }

    private async void SetPedProperty(int networkId, string property, string value)
    {
        var vehicle = await this.FindVehicle(networkId);
        if (vehicle is null)
        {
            return;
        }

        switch (property)
        {
            case FVehiclePropertiesDTO.Events.IsSirenActive:
            {
                vehicle.IsSirenActive = bool.Parse(value);
                break;
            }

            case FVehiclePropertiesDTO.Events.CreateBlip:
            {
                var request = JsonConvert.DeserializeObject<FVehiclePropertiesDTO.CreateBlipRequest>(value);
                vehicle.CreateBlip(request.Sprite, request.Color, request.Text);
                break;
            }

            case FVehiclePropertiesDTO.Events.RemoveBlips:
            {
                vehicle.RemoveBlips();
                break;
            }
        }
    }

    private async void InvokeMethod(string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<FPedPropertiesDTO.InvokeMethodRequest>(serializedRequest);
        var vehicle = await this.FindVehicle(request.NetworkId);
        if (vehicle is null)
        {
            return;
        }

        switch (request.Method)
        {
            case FVehiclePropertiesDTO.Events.MarkAsNoLongerNeeded:
            {
                vehicle.MarkAsNoLongerNeeded();
                break;
            }
        }
    }

    private async Task<Vehicle> FindVehicle(int networkId)
    {
        while (!Cfx.API.NetworkDoesEntityExistWithNetworkId(networkId))
        {
            await BaseScript.Delay(10);
        }

        return Entity.FromNetworkId(networkId) as Vehicle;
    }
}