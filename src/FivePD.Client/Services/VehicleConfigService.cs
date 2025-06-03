using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IVehicleConfigService" />
internal class VehicleConfigService : IVehicleConfigService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private List<int> _policeVehicleModelHashes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleConfigService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public VehicleConfigService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VehicleConfig.Get);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VehicleConfig.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VehicleConfig.Create);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VehicleConfig.Delete);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VehicleConfig.Edit);

        this._eventHandlerDictionary[Events.VehicleConfig.UpdatePoliceVehicleModels] += new Action<string>(this.SetPoliceVehicleModels);
        BaseScript.TriggerServerEvent(Events.VehicleConfig.RequestPoliceVehicleModels, new Action<string>(this.SetPoliceVehicleModels));
    }

    /// <summary>
    /// Checks if the given vehicle is a police vehicle based on its model.
    /// </summary>
    /// <param name="vehicle">The vehicle to check.</param>
    /// <returns>True if the vehicle is a police vehicle, false otherwise.</returns>
    public bool IsPoliceVehicle(Vehicle vehicle)
    {
        return this._policeVehicleModelHashes.Contains(vehicle.Model.Hash);
    }

    private void SetPoliceVehicleModels(string serializedPoliceVehicleModels)
    {
        var models = JsonConvert.DeserializeObject<List<string>>(serializedPoliceVehicleModels);
        this._policeVehicleModelHashes = models.Select(Cfx.API.GetHashKey).ToList();
    }
}