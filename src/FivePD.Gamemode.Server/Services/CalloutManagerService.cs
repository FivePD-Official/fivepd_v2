using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CitizenFX.Core;
using FivePD.Gamemode.Client.DTO;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Server.API;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Gamemode.Server.Services;

#pragma warning disable

public class CalloutManagerService : ICalloutManagerService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly PlayerList _players;
    private readonly ITickQueueService _tickQueueService;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;
    private readonly IConfirmationService _confirmationService;
    private readonly ILogger _logger;

    private readonly List<Type> _calloutTypes = new();
    private readonly List<CalloutHolder> _activeCallouts = new();
    private readonly Stopwatch _nextCalloutTimer = new();
    private int _nextCalloutInMs;

    public CalloutManagerService(
        EventHandlerDictionary eventHandlerDictionary,
        PlayerList players,
        ITickQueueService tickQueueService,
        IConfigService configService,
        IRandomService randomService,
        ILogger logger,
        IConfirmationService confirmationService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._players = players;
        this._tickQueueService = tickQueueService;
        this._configService = configService;
        this._randomService = randomService;
        this._logger = logger;
        this._confirmationService = confirmationService;
    }

    /// <inheritdoc />
    public List<Callout> ActiveCallouts => this._activeCallouts.Select(x => x.Callout).ToList();

    public void RegisterEvents()
    {
        this.RegisterCallouts();
        this._nextCalloutInMs = this._randomService.Random.Next(
            this._configService.GeneralSettings.Callout.MinIntervalInMs,
            this._configService.GeneralSettings.Callout.MaxIntervalInMs);
        this._nextCalloutTimer.Start();
        this._tickQueueService.EnqueueInterval(this.GenerateCallout, 1000);
        this._tickQueueService.EnqueueInterval(this.CheckCallouts, 1000);
    }

    private void RegisterCallouts()
    {
        var callouts = (IEnumerable<Callout>)ServiceLocator.GetServices(typeof(Callout));
        foreach (var callout in callouts)
        {
            this._calloutTypes.Add(callout.GetType());
        }
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

        var calloutType = this._calloutTypes[this._randomService.Random.Next(0, this._calloutTypes.Count)];
        var callout = (Callout)ServiceLocator.GetService(calloutType);
        if (callout is null)
        {
            this._logger.Warning("A callout with the type of \"{Type}\" could not be instantiated.", calloutType);
            return;
        }

        var properties = await callout.Setup();
        callout.Title = properties.Title;
        callout.Priority = properties.Priority;

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
        this._activeCallouts.Add(calloutHolder);
        calloutHolder.Timer.Start();

        foreach (var player in this._players)
        {
            this._confirmationService.TriggerConfirmation(
                player,
                288, // F1
                "accept with f1",
                $"Callout {callout.Title} with priority {callout.Priority}",
                null,
                response =>
                {
                    if (!response)
                    {
                        return;
                    }

                    var fPlayer = FPlayer.FromPlayer(player);
                    ((IInternalIncident)calloutHolder.Callout).Players.Add(fPlayer);
                    callout.OnAccepted(fPlayer);
                    
                    // TODO: trigger event for player with details, location, create blip, etc
                });
        }

        await callout.OnDispatched(this._players.Select(FPlayer.FromPlayer).ToList());
    }

    private void CheckCallouts()
    {
        this._activeCallouts.RemoveAll(x => x.Timer.ElapsedMilliseconds >= 30000 && x.Callout.Players.Count == 0);
        foreach (var calloutHolder in this._activeCallouts)
        {
            if (calloutHolder.Callout.Players.Count > 0)
            {
                calloutHolder.Timer.Stop();
            }
        }
    }

    private sealed record CalloutHolder(Callout Callout, Stopwatch Timer);
}