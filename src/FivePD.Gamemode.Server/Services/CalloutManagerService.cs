using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Gamemode.Client.DTO;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Server.API;
using FivePD.Gamemode.Server.Attributes;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.QueryHelpers;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.Services;

#pragma warning disable

public class CalloutManagerService : ICalloutManagerService, IInternalCalloutManagerService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ITickQueueService _tickQueueService;

    private readonly List<Type> _calloutTypes = new();
    private readonly List<CalloutHolder> _activeCallouts = new();

    public CalloutManagerService(
        EventHandlerDictionary eventHandlerDictionary,
        ITickQueueService tickQueueService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._tickQueueService = tickQueueService;
    }

    List<Type> IInternalCalloutManagerService.CalloutTypes => this._calloutTypes;

    List<CalloutHolder> IInternalCalloutManagerService.ActiveCallouts => this._activeCallouts;

    /// <inheritdoc />
    public List<Callout> ActiveCallouts => this._activeCallouts.Select(x => x.Callout).ToList();

    /// <inheritdoc />
    public Callout GetPlayerCallout(FPlayer player)
    {
        return this.ActiveCallouts.FirstOrDefault(x => x.Units.Any(unit => unit.Players.Any(p => p.NetId == player.NetId)));
    }

    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Callout.OnPlayerEnteredInActivityZone] += new Action<Player, string>(this.OnPlayerEnteredInActivityZone);
        this.RegisterCallouts();
        this._tickQueueService.EnqueueInterval(this.CheckCallouts, 1000);
    }

    public void CompleteCalloutOfPlayer(FPlayer player)
    {
        this.CompleteCallout(this.GetPlayerCallout(player));
    }

    private void RegisterCallouts()
    {
        var callouts = (IEnumerable<Callout>)ServiceLocator.GetServices(typeof(Callout));
        foreach (var callout in callouts)
        {
            this._calloutTypes.Add(callout.GetType());
        }
    }

    private void CheckCallouts()
    {
        this._activeCallouts.RemoveAll(x => x.Timer.ElapsedMilliseconds >= 10000 && x.Callout.Units.Count == 0);
        foreach (var calloutHolder in this._activeCallouts)
        {
            if (calloutHolder.Callout.Units.Count > 0)
            {
                calloutHolder.Timer.Reset();
            }
        }
    }

    private void CompleteCallout(Callout callout)
    {
        if (callout is null)
        {
            return;
        }

        callout.OnCompleted();
        foreach (var unit in callout.Units)
        {
            foreach (var player in unit.Players)
            {
                player.GetUnderlyingPlayer().TriggerEvent(Events.Callout.OnCompleted);
            }
        }

        this._activeCallouts.RemoveAll(x => x.Callout.Id == callout.Id);
    }

    private void OnPlayerEnteredInActivityZone([FromSource] Player player, string calloutId)
    {
        var callout = this._activeCallouts.FirstOrDefault(x => x.Callout.Id.ToString() == calloutId);
        if (callout is null)
        {
            return;
        }

        callout.Callout.OnPlayerEnteredInActivityZone(FPlayer.FromPlayer(player));
    }

    [UIEvent(Events.Callout.GetAllActiveUI)]
    private void GetAllActiveCalloutsUI(ExpandoObject _, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var activeCallouts = this._activeCallouts
            .Where(x => x.Callout.Units.Count > 0)
            .Select(x => new CalloutDTO.CalloutToClientDTO
            {
                Title = x.Callout.Title,
                Description = x.Callout.Description,
                Location = x.Callout.Location,
                Units = x.Callout.Units.Select(p => p.Callsign).ToList(),
            }).ToList();
        var page = new Page<CalloutDTO.CalloutToClientDTO>
        {
            Items = activeCallouts,
            CurrentPage = 1,
            ItemCount = activeCallouts.Count,
            PageCount = 1,
            PageSize = activeCallouts.Count,
        };
        networkCallbackDelegate.InvokeWithJson(page);
    }
}