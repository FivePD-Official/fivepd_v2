using System.Collections.Generic;
using CitizenFX.Core;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Service for handling network events.
/// </summary>
public interface INetworkEventService
{
    /// <summary>
    /// Triggers a network event for a specific player.
    /// </summary>
    /// <param name="player">The player to trigger the event for.</param>
    /// <param name="guid">The unique identifier of the event.</param>
    /// <param name="args">The arguments to pass with the event.</param>
    public void TriggerEvent(Player player, string guid, params object[] args);

    /// <summary>
    /// Triggers a serialized network event for a specific player.
    /// </summary>
    /// <param name="player">The player to trigger the event for.</param>
    /// <param name="guid">The unique identifier of the event.</param>
    /// <param name="obj">The object to serialize and pass with the event.</param>
    public void TriggerEventSerialized(Player player, string guid, object obj);

    /// <summary>
    /// Triggers a network event with a response for a specific player.
    /// </summary>
    /// <param name="player">The player to trigger the event for.</param>
    /// <param name="guid">The unique identifier of the event.</param>
    /// <param name="status">The status of the response.</param>
    /// <param name="title">The title of the response.</param>
    /// <param name="description">The description of the response.</param>
    /// <param name="params">The parameters of the response.</param>
    public void TriggerEventWithResponse(Player player, string guid, bool status, string title, string description, Dictionary<string, string> @params = null);

    /// <summary>
    /// Triggers a network event with an unexpected error for a specific player.
    /// </summary>
    /// <param name="player">The player to trigger the event for.</param>
    /// <param name="guid">The unique identifier of the event.</param>
    public void TriggerEventWithUnexpectedError(Player player, string guid);
}