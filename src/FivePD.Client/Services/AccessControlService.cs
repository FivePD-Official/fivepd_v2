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
    private readonly INetworkEventService _networkEventService;

    public AccessControlService(EventHandlerDictionary eventHandlerDictionary, INetworkEventService networkEventService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._networkEventService = networkEventService;
    }

    public void OnInit()
    {
        this._networkEventService.TriggerServerEvent(Events.AccessControl.HasAccess, new Action<bool>(allowed =>
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
