using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Client.Models;
using FivePD.Common;
using Newtonsoft.Json;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.ILocationService" />
internal class LocationService : ILocationService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public LocationService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public List<Location> Locations { get; private set; }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Location.Create);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Location.Get);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Location.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Location.Delete);
        this._eventHandlerDictionary[Events.Location.Update] += new Action<string>(this.OnLocationUpdate);
    }

    private void OnLocationUpdate(string serializedLocations)
    {
        this.Locations = JsonConvert.DeserializeObject<List<Location>>(serializedLocations);
    }
}