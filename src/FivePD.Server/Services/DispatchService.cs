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
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Requests;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

public class DispatchService : IDispatchService, IInternalDispatchService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly PlayerList _playerList;
    private readonly IInternalCalloutStoreService _calloutStoreService;
    private readonly ICalloutDistributionService _calloutDistributionService;
    private readonly IUnitService _unitService;

    public DispatchService(
        EventHandlerDictionary eventHandlerDictionary,
        PlayerList playerList,
        ICalloutStoreService calloutStoreService,
        ICalloutDistributionService calloutDistributionService,
        IUnitService unitService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._playerList = playerList;
        this._calloutStoreService = (IInternalCalloutStoreService)calloutStoreService;
        this._calloutDistributionService = calloutDistributionService;
        this._unitService = unitService;
    }

    public List<FPlayer> Dispatchers()
    {
        return this._playerList
            .Select(FPlayer.FromPlayer)
            .Where(p => p.Dispatcher.Value)
            .ToList();;
    }

    void IServiceWithEvents.RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Dispatch.EnableForPlayer] += new Action<Player>(this.OnEnableForPlayer);
        this._eventHandlerDictionary[Events.Dispatch.DisableForPlayer] += new Action<Player>(this.OnDisableForPlayer);
        this._eventHandlerDictionary[Events.Dispatch.DismissCallout] += new Action<Player, ExpandoObject>(this.OnCalloutDismissed);

        this._unitService.OnUnitCreated += this.OnUnitCreated;
        this._unitService.OnUnitRemoved += this.OnUnitRemoved;
    }

    [UIEvent(Events.UI.Dispatch.GetIncomingCallouts)]
    private void GetActiveCalloutsUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        networkCallbackDelegate.InvokeWithJson(this._calloutStoreService.ActiveCallouts
            .Where(callout => callout.Callout.Units.Count == 0)
            .Select(x => x.Callout.ToCalloutForDispatcherDTO()).ToList());
    }

    [UIEvent(Events.UI.Dispatch.GetUnits)]
    private void GetUnitsUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var units = this._unitService.Units().Select(unit => unit.ToDispatcherDTO()).ToList();
        networkCallbackDelegate.InvokeWithJson(units);
    }
    
    [UIEvent(Events.Dispatch.AssignCalloutToUnits)]
    private void AssignCalloutToUnitsUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<AssignCalloutToUnitsRequest>();
        var calloutHolder = this._calloutStoreService.ActiveCallouts.FirstOrDefault(x => x.Callout.Id == Guid.Parse(request.CalloutId));
        if (calloutHolder is null)
        {
            return;
        }
        
        foreach (var unit in this._unitService.Units().Where(unit => request.UnitCallsigns.Contains(unit.Callsign)))
        {
            this._calloutDistributionService.AssignUnitToCallout(unit, calloutHolder.Callout);
        }
        
        
        // foreach (var dispatcher in this.Dispatchers())
        // {
        //     dispatcher.GetUnderlyingPlayer().TriggerEvent(Events.Dispatch.OnCalloutAssignedToUnits, JsonConvert.SerializeObject(callout.Callout.ToCalloutForDispatcherDTO()));
        // }

        networkCallbackDelegate.Invoke(null);
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
        foreach (var p in this.Dispatchers())
        {
            p.GetUnderlyingPlayer().TriggerEvent(Events.Dispatch.RemoveCalloutFromDispatcher, request.Id);
        }
    }

    private void OnUnitCreated(object _, Unit unit)
    {
        foreach (var dispatcher in this.Dispatchers())
        {
            dispatcher.GetUnderlyingPlayer().TriggerEvent(Events.Dispatch.OnUnitCreated, JsonConvert.SerializeObject(unit.ToCommonDispatcherDTO()));
        }
    }
    
    private void OnUnitRemoved(object _, Unit unit)
    {
        foreach (var dispatcher in this.Dispatchers())
        {
            dispatcher.GetUnderlyingPlayer().TriggerEvent(Events.Dispatch.OnUnitRemoved, unit.Callsign);
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

            player.TriggerEvent(Events.Dispatch.RemoveCalloutFromDispatcher, callout.Id.ToString());
        }
    }
}