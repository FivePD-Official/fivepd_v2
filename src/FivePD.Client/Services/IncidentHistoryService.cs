using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IIncidentHistoryService" />
public class IncidentHistoryService : IIncidentHistoryService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentHistoryService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public IncidentHistoryService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.IncidentHistory.Get);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.IncidentHistory.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.IncidentHistory.GetAllOfUser);
    }
}