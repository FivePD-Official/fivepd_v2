using System;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Common;
using Newtonsoft.Json;

namespace FivePD.Client.Services;

#pragma warning disable

internal class NetworkEventService : INetworkEventService
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    public NetworkEventService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    public void TriggerServerEvent(string eventName, params object[] args)
    {
        var guid = Guid.NewGuid();
        Delegate callback;

        if (args.Length > 0 && args[args.Length - 1] is Delegate lastArg)
        {
            var newArgs = new object[args.Length - 1 + 1];
            newArgs[0] = guid.ToString();
            args.CopyTo(newArgs, 1);

            callback = lastArg;

            BaseScript.TriggerServerEvent(eventName, newArgs);

            var callbackEventName = $"{Events.NetworkEvent.ClientToServer}:{guid}";

            Action<object> cb = null;
            cb = result =>
            {
                try
                {
                    callback?.DynamicInvoke(result);
                }
                finally
                {
                    if (cb is not null)
                    {
                        this._eventHandlerDictionary[callbackEventName] -= cb;
                    }
                }
            };

            _eventHandlerDictionary[callbackEventName] += cb;
            return;
        }

        BaseScript.TriggerServerEvent(eventName, guid.ToString(), args);
    }
}