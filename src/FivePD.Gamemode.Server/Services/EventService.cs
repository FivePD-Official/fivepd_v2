using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Server.Interfaces;

namespace FivePD.Gamemode.Server.Services;

/// <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.IEventService" />
public class EventService : IEventService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly PlayerList _players;
    private readonly ConcurrentDictionary<string, Action<string>> _events = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EventService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="players">The <see cref="PlayerList"/> to use.</param>
    public EventService(EventHandlerDictionary eventHandlerDictionary, PlayerList players)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._players = players;
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
        // TODO: this might not work, I'm not sure if an Entity.Owner is null if the entity has no owner,
        // also select a random player
        player ??= this._players[0];
        BaseScript.TriggerClientEvent(player, eventName, args);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<string> TriggerEventAsync(Player player, string eventName, params object[] args)
    {
        var value = string.Empty;
        this.TriggerEvent(player, eventName, s => { value = s; }, args);

        // TODO: wait only few seconds, also I'm not sure if this blocks the thread or not, maybe run it in a tick (ITickQueueService)?
        while (value.Length == 0)
        {
            await BaseScript.Delay(0);
        }

        return value;
    }
}