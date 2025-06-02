using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class ConfirmationService : IConfirmationService
{
    private readonly IEventService _eventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmationService"/> class.
    /// </summary>
    /// <param name="eventService">The <see cref="IEventService"/> to use.</param>
    public ConfirmationService(IEventService eventService)
    {
        this._eventService = eventService;
    }

    /// <inheritdoc />
    public void TriggerConfirmation(
        Player player,
        ConfirmationProperties properties,
        Action<bool> callback)
    {
        this._eventService.TriggerEventStrict(
            player,
            Events.Generic.Action,
            response => callback.Invoke(bool.Parse(response)),
            JsonConvert.SerializeObject(properties));
    }

    /// <inheritdoc/>
    public void TriggerConfirmation(
        FPlayer player,
        ConfirmationProperties properties,
        Action<bool> callback)
    {
        this.TriggerConfirmation(player.GetUnderlyingPlayer(), properties, callback);
    }

    /// <inheritdoc/>
    public void TriggerConfirmation(
        Unit unit,
        ConfirmationProperties properties,
        Action<bool> callback)
    {
        foreach (var player in unit.Players)
        {
            this.TriggerConfirmation(player, properties, callback);
        }
    }

    /// <inheritdoc />
    public async Task<bool> TriggerConfirmationAsync(
        Player player,
        ConfirmationProperties properties)
    {
        var result = await this._eventService.TriggerEventStrictAsync(
            player,
            Events.Generic.Action,
            JsonConvert.SerializeObject(properties));
        return bool.Parse(result);
    }

    /// <inheritdoc/>
    public Task<bool> TriggerConfirmationAsync(
        FPlayer player,
        ConfirmationProperties properties)
    {
        return this.TriggerConfirmationAsync(player.GetUnderlyingPlayer(), properties);
    }
}