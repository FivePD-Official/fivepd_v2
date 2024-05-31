using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;

namespace FivePD.Client.Services;

#pragma warning disable

internal class AccessControlService : IAccessControlService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    public AccessControlService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }
    
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AccessControl.AllowUI);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AccessControl.DisallowUI);
    }
}