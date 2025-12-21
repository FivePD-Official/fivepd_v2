using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IEventService" />
[Obfuscate]
public class EventService : IEventService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly PlayerList _players;
    private readonly ConcurrentDictionary<string, Action<string>> _events = new();
    private readonly IRandomService _randomService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="players">The <see cref="PlayerList"/> to use.</param>
    /// <param name="randomService">The <see cref="IRandomService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public EventService(EventHandlerDictionary eventHandlerDictionary, PlayerList players, IRandomService randomService, ILogger logger)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._players = players;
        this._randomService = randomService;
        this._logger = logger;
    }

    /// <inheritdoc />
    void IOnInit.OnInit()
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
        if (player is null)
        {
            this._logger?.Warning($"Tried to trigger event with name \"{eventName}\", but \"player\" was null.");
            return;
        }

        BaseScript.TriggerClientEvent(player, eventName, args);
    }

    /// <inheritdoc />
    public void TriggerEventStrict(Player player, string eventName, Action<string> callback, params object[] args)
    {
        if (player is null)
        {
            this._logger?.Warning($"Tried to trigger event with name \"{eventName}\", but \"player\" was null.");
            return;
        }

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
        var tcs = new TaskCompletionSource<string>();
        this.TriggerEventStrict(player, eventName, s => tcs.TrySetResult(s), args);

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
        if (completedTask == tcs.Task)
        {
            return await tcs.Task;
        }

        this._logger?.Warning(
            "Event \"{EventName}\" timed out after 5 seconds for player \"{PlayerName}\". (Player handle: \"{PlayerHandle}\").",
            eventName,
            player?.Name ?? "unknown",
            player?.Handle ?? "unknown");
        return string.Empty;
    }

    private Player GetRandomPlayer()
    {
        if (this._players is null || !this._players.Any())
        {
            return null;
        }

        var index = this._randomService.Random.Next(0, this._players.Count());
        return this._players.ElementAt(index);
    }
}