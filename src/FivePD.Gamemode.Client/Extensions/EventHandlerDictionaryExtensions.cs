using System;
using System.Dynamic;
using CitizenFX.Core;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Extensions;

/// <summary>
/// Holds extension methods for <see cref="EventHandlerDictionary"/>.
/// </summary>
internal static class EventHandlerDictionaryExtensions
{
    /// <summary>
    /// Registers a Nui event.
    /// </summary>
    /// <param name="eventHandlerDictionary">The event handler dictionary to add the event for.</param>
    /// <param name="eventName">The event's name.</param>
    /// <param name="action">The action that'll be invoked when the event is sent from the Nui.</param>
    /// <typeparam name="T">The type of the request body.</typeparam>
    public static void AddUIEvent<T>(this EventHandlerDictionary eventHandlerDictionary, string eventName, Action<T, CallbackDelegate> action)
    {
        Cfx.API.RegisterNuiCallbackType(eventName);
        eventHandlerDictionary[$"__cfx_nui:{eventName}"] += action;
    }

    /// <summary>
    /// Registers a NUI event without a custom action.
    /// The data will be sent directly to the server,
    /// then the response back to the NUI.
    /// </summary>
    /// <param name="eventHandlerDictionary">The event handler dictionary to add the event for.</param>
    /// <param name="eventName">The event's name.</param>
    /// <param name="onResponse">This action will get invoked with the response from the server.</param>
    public static void AddUIToServerEvent(this EventHandlerDictionary eventHandlerDictionary, string eventName, Action<string> onResponse = null)
    {
        Cfx.API.RegisterNuiCallbackType(eventName);
        eventHandlerDictionary[$"__cfx_nui:{eventName}"] += new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
        {
            BaseScript.TriggerServerEvent(eventName, body, new Action<string>(data =>
            {
                onResponse?.Invoke(data);
                cb.Invoke(data);
            }));
        });
    }
}