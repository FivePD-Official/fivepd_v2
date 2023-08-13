using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Server.API;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Requests;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

public class DispatchService : IDispatchService, IInternalDispatchService, IServiceWithEvents
{
    private readonly List<Callout> _sentCallouts = new();

    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly PlayerList _playerList;
    private readonly IInternalCalloutStoreService _calloutStoreService;
    private readonly IUnitService _unitService;

    public DispatchService(
        EventHandlerDictionary eventHandlerDictionary,
        PlayerList playerList,
        ICalloutStoreService calloutStoreService,
        IUnitService unitService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._playerList = playerList;
        this._calloutStoreService = (IInternalCalloutStoreService)calloutStoreService;
        this._unitService = unitService;
    }

    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Dispatch.EnableForPlayer] += new Action<Player>(this.OnEnableForPlayer);
        this._eventHandlerDictionary[Events.Dispatch.DisableForPlayer] += new Action<Player>(this.OnDisableForPlayer);
        this._eventHandlerDictionary[Events.Dispatch.DismissCallout] += new Action<Player, ExpandoObject>(this.OnCalloutDismissed);
    }

    [UIEvent(Events.UI.Dispatch.GetActiveCallouts)]
    private void GetActiveCalloutsUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        networkCallbackDelegate.InvokeWithJson(this._calloutStoreService.ActiveCallouts.Select(x => x.Callout.ToCalloutForDispatcherDTO()).ToList());
    }

    [UIEvent(Events.UI.Dispatch.GetUnits)]
    private void GetUnitsUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var units = this._unitService.Units().Select(unit => new DispatcherUnitDTO
        {
            Callsign = unit.Callsign,
            Players = unit.Players.Select(player => new SimplePlayerDTO(player.NetId, player.GetUnderlyingPlayer().Name)),
            Availability = unit.Availability.Value,
            Status = unit.Status.Value,
            AttachedCallout = unit.Callout is null ? null : new SimpleCallout(((IInternalUnit)unit).InternalCallout),
        }).ToList();
        networkCallbackDelegate.InvokeWithJson(units);
    }

    private void OnEnableForPlayer([FromSource] Player player)
    {
        var fPlayer = FPlayer.FromPlayer(player);
        fPlayer.Dispatcher.Value = true;
    }

    private void OnDisableForPlayer([FromSource] Player player)
    {
        var fPlayer = FPlayer.FromPlayer(player);
        fPlayer.Dispatcher.Value = false;
    }

    private void OnCalloutDismissed([FromSource] Player player, ExpandoObject expandoObject)
    {
        var request = expandoObject.ConvertTo<StringIdRequest>();
        this._calloutStoreService.ActiveCallouts.RemoveAll(x => x.Callout.Id == Guid.Parse(request.Id));
        foreach (var p in this._playerList)
        {
            var fPlayer = FPlayer.FromPlayer(p);
            if (!fPlayer.Dispatcher.Value)
            {
                continue;
            }

            player.TriggerEvent(Events.Dispatch.RemoveCalloutFromDispatcher, request.Id);
        }
    }

    void IInternalDispatchService.SendCalloutToDispatchers(Callout callout)
    {
        var sentToAnyPlayer = false;
        foreach (var player in this._playerList)
        {
            var fPlayer = FPlayer.FromPlayer(player);
            if (!fPlayer.Dispatcher.Value)
            {
                continue;
            }

            sentToAnyPlayer = true;
            this._sentCallouts.Add(callout);
            player.TriggerEvent(Events.Dispatch.SendCalloutToDispatcher, JsonConvert.SerializeObject(callout.ToCalloutForDispatcherDTO()));
        }

        if (!sentToAnyPlayer)
        {
            this._calloutStoreService.ActiveCallouts.RemoveAll(x => x.Callout == callout);
        }
    }

    void IInternalDispatchService.RemoveCalloutFromDispatchers(Callout callout)
    {
        foreach (var player in this._playerList)
        {
            var fPlayer = FPlayer.FromPlayer(player);
            if (!fPlayer.Dispatcher.Value)
            {
                continue;
            }

            this._sentCallouts.Remove(callout);
            player.TriggerEvent(Events.Dispatch.RemoveCalloutFromDispatcher, callout.Id.ToString());
        }
    }
}