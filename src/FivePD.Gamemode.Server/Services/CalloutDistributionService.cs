using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CitizenFX.Core;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Common.Models;
using FivePD.Gamemode.Server.API;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Gamemode.Server.Services;

#pragma warning disable

internal class CalloutDistributionService : ICalloutDistributionService, IServiceWithEvents
{
    private readonly PlayerList _players;
    private readonly ITickQueueService _tickQueueService;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;
    private readonly IConfirmationService _confirmationService;
    private readonly ILogger _logger;
    private readonly IInternalCalloutManagerService _calloutManagerService;

    private readonly Stopwatch _nextCalloutTimer = new();
    private int _nextCalloutInMs;

    public CalloutDistributionService(
        PlayerList players,
        ITickQueueService tickQueueService,
        IConfigService configService,
        IRandomService randomService,
        ILogger logger,
        IConfirmationService confirmationService,
        IInternalCalloutManagerService calloutManagerService)
    {
        this._players = players;
        this._tickQueueService = tickQueueService;
        this._configService = configService;
        this._randomService = randomService;
        this._logger = logger;
        this._confirmationService = confirmationService;
        this._calloutManagerService = calloutManagerService;
    }

    public void RegisterEvents()
    {
        this._nextCalloutInMs = this._randomService.Random.Next(
            this._configService.GeneralSettings.Callout.MinIntervalInMs,
            this._configService.GeneralSettings.Callout.MaxIntervalInMs);
        this._nextCalloutTimer.Start();
        this._tickQueueService.EnqueueInterval(this.GenerateCallout, 1000);
    }

    private async void GenerateCallout()
    {
        if (this._nextCalloutTimer.ElapsedMilliseconds < this._nextCalloutInMs)
        {
            return;
        }

        this._nextCalloutInMs = this._randomService.Random.Next(
            this._configService.GeneralSettings.Callout.MinIntervalInMs,
            this._configService.GeneralSettings.Callout.MaxIntervalInMs);
        this._nextCalloutTimer.Restart();

        var calloutType = this._calloutManagerService.CalloutTypes[this._randomService.Random.Next(0, this._calloutManagerService.CalloutTypes.Count)];
        var callout = (Callout)ServiceLocator.GetService(calloutType);
        if (callout is null)
        {
            this._logger.Warning("A callout with the type of \"{Type}\" could not be instantiated.", calloutType);
            return;
        }

        var properties = await callout.Setup();
        callout.Id = Guid.NewGuid();
        callout.Title = properties.Title;
        callout.Description = properties.Description;
        callout.Priority = properties.Priority;
        callout.ActivityRadius = properties.ActivityRadius;

        if (properties.Locations is not null)
        {
            var locations = properties.Locations.ToList();
            var locationsCount = locations.Count;
            if (locationsCount > 0)
            {
                callout.Location = locations[this._randomService.Random.Next(0, locationsCount)];
            }
        }
        else
        {
            callout.Location = properties.Location;
        }

        var canBeDispatched = await callout.CanBeDispatched();
        if (!canBeDispatched)
        {
            return;
        }

        var calloutHolder = new CalloutHolder(callout, new Stopwatch());
        this._calloutManagerService.ActiveCallouts.Add(calloutHolder);
        calloutHolder.Timer.Start();

        var filteredPlayers = this._players.Where(p =>
            FPlayer.FromPlayer(p).DutyStatus.Value != FDutyStatus.OffDuty &&
            this._calloutManagerService.ActiveCallouts.All(c => c.Callout.Players.All(cp => cp.NetId != p.Handle)));
        foreach (var player in filteredPlayers)
        {
            this._confirmationService.TriggerConfirmation(
                player,
                new ConfirmationProperties
                {
                    Notification = new ConfirmationProperties.NotificationProperties
                    {
                        Texture = "CHAR_CALL911",
                        Sender = "LABEL.DISPATCH",
                        Subject = "LABEL.CALLOUT",
                        Content = $"{callout.Title}~n~{{{{priority}}}}: {{{{priorityCode}}}}~n~{{{{location}}}}: {{{{location}}}}",
                        ContentParams = new Dictionary<string, string>
                        {
                            ["priority"] = "LABEL.PRIORITY.TITLE",
                            ["priorityCode"] = callout.Priority switch
                            {
                                FIncidentPriority.Code1 => "LABEL.PRIORITY.CODE1",
                                FIncidentPriority.Code2 => "LABEL.PRIORITY.CODE2",
                                FIncidentPriority.Code3 => "LABEL.PRIORITY.CODE3",
                            },
                        }
                    }
                },
                response =>
                {
                    if (!response)
                    {
                        return;
                    }

                    var fPlayer = FPlayer.FromPlayer(player);
                    ((IInternalIncident)calloutHolder.Callout).Players.Add(fPlayer);
                    callout.OnAccepted(fPlayer);

                    player.TriggerEvent(Events.Callout.OnAccepted, JsonConvert.SerializeObject(new ClientCallout
                    {
                        Id = callout.Id.ToString(),
                        Title = callout.Title,
                        Description = callout.Description,
                        Location = callout.Location,
                        Priority = callout.Priority,
                        ActivityRadius = callout.ActivityRadius,
                    }));
                });
        }

        await callout.OnDispatched(this._players.Select(FPlayer.FromPlayer).ToList());
    }
}