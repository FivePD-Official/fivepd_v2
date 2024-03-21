using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IVehicleConfigService" />
internal class VehicleConfigService : IVehicleConfigService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

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
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VehicleConfig.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VehicleConfig.Add);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VehicleConfig.Delete);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VehicleConfig.Edit);
    }
}