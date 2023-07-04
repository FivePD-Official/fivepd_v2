using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Common;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IEventService" />
public class EventService : IEventService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly PlayerList _players;
    private readonly ConcurrentDictionary<string, Action<string>> _events = new();
    private readonly IRandomService _randomService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="players">The <see cref="PlayerList"/> to use.</param>
    /// <param name="randomService">The <see cref="IRandomService"/> to use.</param>
    public EventService(EventHandlerDictionary eventHandlerDictionary, PlayerList players, IRandomService randomService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._players = players;
        this._randomService = randomService;
    }

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Generic.Callback] += new Action<string, string>((eventId, value) =>
        {
            this._events.TryRemove(eventId, out var action);
            if (action is null)
            {
                return;
            }

            action.Invoke(value);
        });
    }

    /// <inheritdoc />
    public void TriggerEventWithoutCallback(Player player, string eventName, params object[] args)
    {
        player ??= this.GetRandomPlayer();
        this.TriggerEventStrictWithoutCallback(player, eventName, args);
    }

    /// <inheritdoc />
    public void TriggerEvent(Player player, string eventName, Action<string> callback, params object[] args)
    {
        player ??= this.GetRandomPlayer();
        this.TriggerEventStrict(player, eventName, callback, args);
    }

    /// <inheritdoc />
    public Task<string> TriggerEventAsync(Player player, string eventName, params object[] args)
    {
        player ??= this.GetRandomPlayer();
        return this.TriggerEventStrictAsync(player, eventName, args);
    }

    /// <inheritdoc />
    public void TriggerEventStrictWithoutCallback(Player player, string eventName, params object[] args)
    {
        BaseScript.TriggerClientEvent(player, eventName, args);
    }

    /// <inheritdoc />
    public void TriggerEventStrict(Player player, string eventName, Action<string> callback, params object[] args)
    {
        var guid = Guid.NewGuid().ToString();
        this._events.TryAdd(guid, callback);
        var newArgs = new object[args.Length + 1];
        newArgs[0] = guid;
        args.CopyTo(newArgs, 1);
        BaseScript.TriggerClientEvent(player, eventName, newArgs);
    }

    /// <inheritdoc />
    public async Task<string> TriggerEventStrictAsync(Player player, string eventName, params object[] args)
    {
        var value = string.Empty;
        this.TriggerEventStrict(player, eventName, s => { value = s; }, args);

        // TODO: wait only few seconds, also I'm not sure if this blocks the thread or not, maybe run it in a tick (ITickQueueService)?
        while (value.Length == 0)
        {
            await BaseScript.Delay(0);
        }

        return value;
    }

    private Player GetRandomPlayer()
    {
        var index = this._randomService.Random.Next(0, this._players.Count());
        return this._players.ElementAt(index);
    }
}