using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.API;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

public class DispatchService : IDispatchService, IServiceWithEvents
{
    private readonly List<Callout> _sentCallouts = new();

    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly PlayerList _playerList;

    public DispatchService(EventHandlerDictionary eventHandlerDictionary, PlayerList playerList)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._playerList = playerList;
    }

    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Dispatch.EnableForPlayer] += new Action<Player>(this.OnEnableForPlayer);
        this._eventHandlerDictionary[Events.Dispatch.DisableForPlayer] += new Action<Player>(this.OnDisableForPlayer);
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

    void IDispatchService.SendCalloutToDispatchers(Callout callout)
    {
        foreach (var player in this._playerList)
        {
            var fPlayer = FPlayer.FromPlayer(player);
            if (!fPlayer.Dispatcher.Value)
            {
                continue;
            }

            this._sentCallouts.Add(callout);
            player.TriggerEvent(Events.Dispatch.SendCalloutToDispatcher, JsonConvert.SerializeObject(callout.ToCalloutForDispatcherDTO()));
        }
    }
    
    void IDispatchService.RemoveCalloutFromDispatchers(Callout callout)
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