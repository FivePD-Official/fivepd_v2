using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Client.DTO;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Models;
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
    private readonly ILocalPlayerService _localPlayerService;
    private readonly ITickService _tickService;
    private readonly PlayerList _playerList;

    private Guid _uiPlayerUpdateIntervalId;

    public DispatchService(
        EventHandlerDictionary eventHandlerDictionary,
        IUIManagerService uiManagerService,
        ILocalPlayerService localPlayerService,
        ITickService tickService,
        PlayerList playerList)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._uiManagerService = uiManagerService;
        this._localPlayerService = localPlayerService;
        this._tickService = tickService;
        this._playerList = playerList;
    }

    public void RegisterEvents()
    {
        this._localPlayerService.Player.Dispatcher.Subscribe(dispatcher =>
        {
            if (dispatcher)
            {
                this._uiPlayerUpdateIntervalId = this._tickService.EnqueueInterval(() =>
                {
                    var players = this._playerList
                        .Where(p => !string.IsNullOrEmpty(FPlayer.FromNetId(p.ServerId).UnitCallsign.Value))
                        .Select(p => new SimplePlayer(p.ServerId, p.Character.Position))
                        .ToList();
                    var peds = World.GetAllPeds()
                        .Where(x => x.State["asd"] is not null && x.State["asd"] == true)
                        .Select(p => new SimplePlayer(p.NetworkId, p.Position))
                        .ToList();
                    players.AddRange(peds);
                    this._uiManagerService.SendMessage(new JObject
                    {
                        { "Event", Events.UI.Dispatch.UpdatePlayers },
                        { "Players", JArray.FromObject(players) }
                    });
                }, 5000);
            }
            else
            {
                this._tickService.DequeueInterval(this._uiPlayerUpdateIntervalId);
            }
        });

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
        this._eventHandlerDictionary[Events.Dispatch.OnUnitCreated] += new Action<string>(this.OnUnitCreated);
        this._eventHandlerDictionary[Events.Dispatch.OnUnitRemoved] += new Action<string>(this.OnUnitRemoved);

        this._eventHandlerDictionary.AddUIToServerEventOverrideData(Events.UI.Dispatch.GetIncomingCallouts, calloutForDispatcherDtoList =>
        {
            var callout = JsonConvert.DeserializeObject<List<DispatcherCalloutDTO>>(calloutForDispatcherDtoList);
            var callouts = callout.Select(x => new DispatcherCalloutDTOClientWrapper(x, x.Location.GetStreetName())).ToList();
            return JsonConvert.SerializeObject(callouts);
        });
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Dispatch.GetUnits);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Dispatch.AssignCalloutToUnits);
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
            { "Event", Events.Dispatch.RemoveCalloutFromDispatcher },
            { "CalloutId", calloutId },
        });
    }

    private void OnUnitCreated(string serializedUnit)
    {
        var unit = JsonConvert.DeserializeObject<CommonDispatcherUnitDTO>(serializedUnit);
        var wrapper = new CommonDispatcherUnitDTOClientWrapper(unit);
        this._uiManagerService.SendMessage(JsonConvert.SerializeObject(wrapper));
    }

    private void OnUnitRemoved(string unitCallsign)
    {
        this._uiManagerService.SendMessage(new JObject
        {
            { "Event", Events.Dispatch.OnUnitRemoved },
            { "Callsign", unitCallsign },
        });
    }
}