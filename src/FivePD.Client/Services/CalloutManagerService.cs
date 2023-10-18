using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.ICalloutManagerService" />
internal class CalloutManagerService : ICalloutManagerService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalloutManagerService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public CalloutManagerService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Callout.GetAllTypes);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Callout.Dispatch);

        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.CalloutDetails.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.CalloutDetails.Get);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.CalloutDetails.UpdateRuleset);
    }
}