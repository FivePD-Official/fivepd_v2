using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Common;
using FivePD.Common.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FivePD.Client.Services;

#pragma warning disable

internal class DispatchService : IDispatchService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IUIManagerService _uiManagerService;

    public DispatchService(
        EventHandlerDictionary eventHandlerDictionary,
        IUIManagerService uiManagerService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._uiManagerService = uiManagerService;
    }

    public void RegisterEvents()
    {
        this._eventHandlerDictionary.AddUIEvent(Events.UI.Dispatch.Enable,
            new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
            {
                BaseScript.TriggerServerEvent(Events.Dispatch.EnableForPlayer);
                cb.Invoke(string.Empty);
            }));

        this._eventHandlerDictionary.AddUIEvent(Events.UI.Dispatch.Disable,
            new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
            {
                BaseScript.TriggerServerEvent(Events.Dispatch.DisableForPlayer);
                cb.Invoke(string.Empty);
            }));

        this._eventHandlerDictionary.AddUIEvent(Events.Dispatch.DismissCallout,
            new Action<ExpandoObject, CallbackDelegate>((body, cb) =>
            {
                BaseScript.TriggerServerEvent(Events.Dispatch.DismissCallout, body);
                cb.Invoke(string.Empty);
            }));

        this._eventHandlerDictionary[Events.Dispatch.SendCalloutToDispatcher] += new Action<string>(this.OnCalloutReceived);
        this._eventHandlerDictionary[Events.Dispatch.RemoveCalloutFromDispatcher] += new Action<string>(this.OnCalloutRemove);
        
        this._eventHandlerDictionary.AddUIToServerEventOverrideData(Events.UI.Dispatch.GetActiveCallouts, calloutForDispatcherDtoList =>
        {
            var callout = JsonConvert.DeserializeObject<List<DispatcherCalloutDTO>>(calloutForDispatcherDtoList);
            var callouts = callout.Select(x => new DispatcherCalloutDTOClientWrapper(x, x.Location.GetStreetName())).ToList();
            return JsonConvert.SerializeObject(callouts);
        });
    }

    private void OnCalloutReceived(string calloutForDispatcherDto)
    {
        var callout = JsonConvert.DeserializeObject<DispatcherCalloutDTO>(calloutForDispatcherDto);
        var wrapper = new DispatcherCalloutDTOClientWrapper(callout, callout.Location.GetStreetName());
        this._uiManagerService.SendMessage(JsonConvert.SerializeObject(wrapper));
    }

    private void OnCalloutRemove(string calloutId)
    {
        this._uiManagerService.SendMessage(new JObject
        {
            { "type", Events.Dispatch.RemoveCalloutFromDispatcher },
            { "CalloutId", calloutId },
        });
    }
}