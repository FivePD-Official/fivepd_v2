using System;
using CitizenFX.Core;
using FivePD.Client.DTO;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc />
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
}