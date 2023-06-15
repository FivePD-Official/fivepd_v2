using System;
using CitizenFX.Core;
using FivePD.Gamemode.Client.DTO;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Models;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.Services;

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