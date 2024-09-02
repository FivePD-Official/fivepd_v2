using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
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
        Action<bool> callback,
        int timeoutInMs = 5000)
    {
        this._eventService.TriggerEventStrict(
            player,
            Events.Generic.Action,
            response => callback.Invoke(bool.Parse(response)),
            JsonConvert.SerializeObject(new ConfirmationDTO
            {
                TimeoutInMs = timeoutInMs,
                Properties = properties,
            }));
    }

    /// <inheritdoc/>
    public void TriggerConfirmation(
        FPlayer player,
        ConfirmationProperties properties,
        Action<bool> callback,
        int timeoutInMs = 5000)
    {
        this.TriggerConfirmation(player.GetUnderlyingPlayer(), properties, callback, timeoutInMs);
    }

    /// <inheritdoc/>
    public void TriggerConfirmation(
        Unit unit,
        ConfirmationProperties properties,
        Action<bool> callback,
        int timeoutInMs = 5000)
    {
        foreach (var player in unit.Players)
        {
            this.TriggerConfirmation(player, properties, callback, timeoutInMs);
        }
    }

    /// <inheritdoc />
    public async Task<bool> TriggerConfirmationAsync(
        Player player,
        ConfirmationProperties properties,
        int timeoutInMs = 5000)
    {
        var result = await this._eventService.TriggerEventStrictAsync(
            player,
            Events.Generic.Action,
            JsonConvert.SerializeObject(new ConfirmationDTO
            {
                TimeoutInMs = timeoutInMs,
                Properties = properties,
            }));

        return bool.Parse(result);
    }

    /// <inheritdoc/>
    public Task<bool> TriggerConfirmationAsync(
        FPlayer player,
        ConfirmationProperties properties,
        int timeoutInMs = 5000)
    {
        return this.TriggerConfirmationAsync(player.GetUnderlyingPlayer(), properties, timeoutInMs);
    }
}