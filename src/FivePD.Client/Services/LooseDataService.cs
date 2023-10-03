using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.ILooseDataService" />
public class LooseDataService : ILooseDataService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="LooseDataService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public LooseDataService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LooseData.GetEntity);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LooseData.GetEntities);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LooseData.AddEntity);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LooseData.EditEntity);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LooseData.DeleteEntity);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LooseData.GetSetAttribute);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LooseData.GetSetAttributes);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LooseData.GetSetAttributesPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LooseData.AddSetAttribute);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LooseData.EditSetAttribute);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LooseData.DeleteSetAttribute);
    }
}