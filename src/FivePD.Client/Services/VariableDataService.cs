using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;

namespace FivePD.Client.Services;

/// <inheritdoc cref="IVariableDataService" />
public class VariableDataService : IVariableDataService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableDataService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public VariableDataService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VariableData.GetEntity);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VariableData.GetEntitiesPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VariableData.GetEntities);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VariableData.AddEntity);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VariableData.EditEntity);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VariableData.DeleteEntity);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VariableData.GetSetAttribute);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VariableData.GetSetAttributes);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VariableData.GetSetAttributesPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VariableData.AddSetAttribute);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VariableData.EditSetAttribute);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.VariableData.DeleteSetAttribute);
    }
}