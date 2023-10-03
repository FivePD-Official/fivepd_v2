using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IUnitService" />
public class UnitService : IUnitService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public UnitService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Unit.GetAll);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Unit.Join);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Unit.RequestToJoin);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Unit.Leave);
    }
}