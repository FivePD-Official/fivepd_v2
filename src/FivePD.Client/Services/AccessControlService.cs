using System;
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
        BaseScript.TriggerServerEvent(Events.AccessControl.HasAccess, new Action<bool>(allowed =>
        {
            if (allowed)
            {
                return;
            }

            LifecycleHandler.Stop("You do not have access to use FivePD. Stopping FivePD.");
        }));
        
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AccessControl.GiveAccessUI);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.AccessControl.RemoveAccessUI);
    }
}