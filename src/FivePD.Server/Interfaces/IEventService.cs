using System;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides utility methods to trigger events on the client.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Triggers an event for the specified player or if null selects a random player on the server.
    /// </summary>
    /// <param name="player">The player to trigger the event for.</param>
    /// <param name="eventName">The event name.</param>
    /// <param name="args">The arguments of the event.</param>
    public void TriggerEventWithoutCallback(Player player, string eventName, params object[] args);

    /// <summary>
    /// Triggers an event for the specified player or if null selects a random player on the server.
    /// </summary>
    /// <param name="player">The player to trigger the event for.</param>
    /// <param name="eventName">The event name.</param>
    /// <param name="callback">A callback function which is invoked when the event returns.</param>
    /// <param name="args">The arguments of the event.</param>
    public void TriggerEvent(Player player, string eventName, Action<string> callback, params object[] args);

    /// <summary>
    /// Triggers an event for the specified player or if null selects a random player on the server.
    /// Same as the normal <see cref="TriggerEvent(Player, string, Action{string}, object[])"/> but awaitable for the returned value.
    /// </summary>
    /// <param name="player">The player to trigger the event for.</param>
    /// <param name="eventName">The event name.</param>
    /// <param name="args">The arguments of the event.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<string> TriggerEventAsync(Player player, string eventName, params object[] args);

    /// <summary>
    /// Triggers an event for the specified player.
    /// </summary>
    /// <param name="player">The player to trigger the event for.</param>
    /// <param name="eventName">The event name.</param>
    /// <param name="args">The arguments of the event.</param>
    public void TriggerEventStrictWithoutCallback(Player player, string eventName, params object[] args);

    /// <summary>
    /// Triggers an event for the specified player.
    /// </summary>
    /// <param name="player">The player to trigger the event for.</param>
    /// <param name="eventName">The event name.</param>
    /// <param name="callback">A callback function which is invoked when the event returns.</param>
    /// <param name="args">The arguments of the event.</param>
    public void TriggerEventStrict(Player player, string eventName, Action<string> callback, params object[] args);

    /// <summary>
    /// Triggers an event for the specified player.
    /// Same as the normal <see cref="TriggerEvent(Player, string, Action{string}, object[])"/> but awaitable for the returned value.
    /// </summary>
    /// <param name="player">The player to trigger the event for.</param>
    /// <param name="eventName">The event name.</param>
    /// <param name="args">The arguments of the event.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<string> TriggerEventStrictAsync(Player player, string eventName, params object[] args);
}