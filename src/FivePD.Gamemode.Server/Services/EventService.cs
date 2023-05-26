using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Server.Interfaces;

namespace FivePD.Gamemode.Server.Services;

#pragma warning disable

public class EventService : IEventService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly PlayerList _players;
    private readonly ConcurrentDictionary<string, Action<string>> _events = new();

    public EventService(EventHandlerDictionary eventHandlerDictionary, PlayerList players)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._players = players;
    }

    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Generic.Callback] += new Action<string, string>((eventId, v) =>
        {
            this._events.TryRemove(eventId, out var action);
            if (action is null)
            {
                return;
            }

            action.Invoke(v);
        });
    }

    public void TriggerEventWithoutCallback(Player player, string eventName, params object[] args)
    {
        player ??= this._players[0];
        BaseScript.TriggerClientEvent(player, eventName, args);
    }

    public void TriggerEvent(Player player, string eventName, Action<string> callback, params object[] args)
    {
        var guid = Guid.NewGuid().ToString();
        this._events.TryAdd(guid, callback);
        var newArgs = new object[args.Length + 1];
        newArgs[0] = guid;
        args.CopyTo(newArgs, 1);

        player ??= this._players[0];
        BaseScript.TriggerClientEvent(player, eventName, newArgs);
    }

    public async Task<string> TriggerEventAsync(Player player, string eventName, params object[] args)
    {
        var value = string.Empty;
        this.TriggerEvent(player, eventName, s => { value = s; }, args);

        while (value.Length == 0)
        {
            await BaseScript.Delay(0);
        }

        return value;
    }
}