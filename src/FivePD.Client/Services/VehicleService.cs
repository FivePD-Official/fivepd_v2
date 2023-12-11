using System;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Client.Models;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.DTO;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable VSTHRD100
#pragma warning disable S3168

/// <inheritdoc cref="FivePD.Client.Interfaces.IVehicleService" />
internal class VehicleService : IVehicleService, IOnInit
{
    private readonly IPedService _pedService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleService"/> class.
    /// </summary>
    /// <param name="pedService">The <see cref="IPedService"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public VehicleService(IPedService pedService, EventHandlerDictionary eventHandlerDictionary)
    {
        this._pedService = pedService;
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary[Events.Vehicle.GetProperty] += new Action<string, int, string>(this.GetPedProperty);
        this._eventHandlerDictionary[Events.Vehicle.SetProperty] += new Action<int, string, string>(this.SetPedProperty);
        this._eventHandlerDictionary[Events.Vehicle.InvokeMethod] += new Action<string>(this.InvokeMethod);
        this._eventHandlerDictionary[Events.Vehicle.SetCosmetics] += new Action<string>(this.SetCosmetics);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Vehicle.GetAllPageable);
    }

    /// <inheritdoc />
    public FVehicle GenerateVehicle(Vehicle vehicle)
    {
        var fVehicle = new FVehicle(vehicle);
        if (fVehicle.HasGeneratedData.Value)
        {
            return fVehicle;
        }

        foreach (var ped in vehicle.Occupants)
        {
            this._pedService.GeneratePed(ped);
        }

        var fVehicleBuilder = new CommonFVehicleBuilder(vehicle.NetworkId)
            .WithOwner(vehicle.Driver.NetworkId)
            .WithPassengers(vehicle.Occupants.Select(ped => ped.NetworkId).ToList())
            .WithColor(vehicle.GetColorName())
            .WithBrand(vehicle.DisplayName)
            .WithLicensePlate(Cfx.API.GetVehicleNumberPlateText(vehicle.Handle));

        BaseScript.TriggerServerEvent(Events.Vehicle.Generate, JsonConvert.SerializeObject(fVehicleBuilder));

        return fVehicle;
    }

    private async void GetPedProperty(string eventCallbackId, int networkId, string property)
    {
        var vehicle = await this.FindVehicleAsync(networkId);
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
        var vehicle = await this.FindVehicleAsync(networkId);
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
        var vehicle = await this.FindVehicleAsync(request.NetworkId);
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

    private void SetCosmetics(string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<FVehiclePropertiesDTO.SetCosmetics>(serializedRequest);
        var vehicle = Entity.FromNetworkId(request.NetworkId);
        if (vehicle is null)
        {
            return;
        }

        if (request.Extras.Count != 0)
        {
            for (var i = 1; i < 13; i++)
            {
                Cfx.API.SetVehicleExtra(vehicle.Handle, i, true);
            }

            foreach (var extra in request.Extras.Where(x => Cfx.API.DoesExtraExist(vehicle.Handle, x)))
            {
                Cfx.API.SetVehicleExtra(vehicle.Handle, extra, false);
            }
        }

        Cfx.API.SetVehicleLivery(vehicle.Handle, request.Livery);
    }

    private async Task<Vehicle> FindVehicleAsync(int networkId)
    {
        while (!Cfx.API.NetworkDoesEntityExistWithNetworkId(networkId))
        {
            await BaseScript.Delay(10);
        }

        return Entity.FromNetworkId(networkId) as Vehicle;
    }
}