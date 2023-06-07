using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Gamemode.Client.DTO;
using FivePD.Gamemode.Common;
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
        int key,
        string tooltip,
        string notification,
        Dictionary<string, string> notificationParams,
        Action<bool> callback,
        int timeoutInMs = 5000)
    {
        this._eventService.TriggerEventStrict(
            player,
            Events.Generic.Action,
            response => callback.Invoke(bool.Parse(response)),
            JsonConvert.SerializeObject(new ConfirmationDTO
            {
                Key = key,
                Tooltip = tooltip,
                Notification = notification,
                NotificationParams = notificationParams,
                TimeoutInMs = timeoutInMs,
            }));
    }
}