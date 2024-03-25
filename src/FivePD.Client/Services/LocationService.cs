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

#pragma warning disable

/// <inheritdoc cref="FivePD.Client.Interfaces.ILocationService" />
internal class LocationService : ILocationService, IOnInit
{
    private readonly IPedService _pedService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public LocationService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Location.Create);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Location.Get);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Location.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Location.Delete);
    }
}