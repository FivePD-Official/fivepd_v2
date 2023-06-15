using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Common.Models;
using FivePD.Gamemode.Server.API;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Gamemode.Server.Services;

/// <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.ICalloutDistributionService" />
internal class CalloutDistributionService : ICalloutDistributionService, IServiceWithEvents
{
    private readonly ITickQueueService _tickQueueService;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;
    private readonly IConfirmationService _confirmationService;
    private readonly ILogger _logger;
    private readonly IInternalCalloutManagerService _calloutManagerService;
    private readonly IUnitService _unitService;

    private readonly Stopwatch _nextCalloutTimer = new();
    private int _nextCalloutInMs;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalloutDistributionService"/> class.
    /// </summary>
    /// <param name="tickQueueService">The <see cref="ITickQueueService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="randomService">The <see cref="IRandomService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="confirmationService">The <see cref="IConfirmationService"/> to use.</param>
    /// <param name="calloutManagerService">The <see cref="ICalloutManagerService"/> to use.</param>
    /// <param name="unitService">The <see cref="IUnitService"/> to use.</param>
    public CalloutDistributionService(
        ITickQueueService tickQueueService,
        IConfigService configService,
        IRandomService randomService,
        ILogger logger,
        IConfirmationService confirmationService,
        IInternalCalloutManagerService calloutManagerService,
        IUnitService unitService)
    {
        this._tickQueueService = tickQueueService;
        this._configService = configService;
        this._randomService = randomService;
        this._logger = logger;
        this._confirmationService = confirmationService;
        this._calloutManagerService = calloutManagerService;
        this._unitService = unitService;
    }

    /// <inheritdoc/>
    public void RegisterEvents()
    {
        this._nextCalloutInMs = this._randomService.Random.Next(
            this._configService.GeneralSettings.Callout.MinIntervalInMs,
            this._configService.GeneralSettings.Callout.MaxIntervalInMs);
        this._nextCalloutTimer.Start();
        this._tickQueueService.EnqueueInterval(this.GenerateCalloutAsync, 1000);
    }

    private async Task GenerateCalloutAsync()
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

        var dispatchedUnits = this.DispatchCallout(calloutHolder);
        await callout.OnDispatched(dispatchedUnits);
    }

    private List<Unit> DispatchCallout(CalloutHolder calloutHolder)
    {
        var filteredUnits = this._unitService.Units
            .Where(unit =>
                this._calloutManagerService.ActiveCallouts.All(holder => holder.Callout.Units.All(u => u.Callsign != unit.Callsign)))
            .ToList();
        foreach (var unit in filteredUnits)
        {
            var unitAccepted = false;

            void OnAccepted()
            {
                if (unitAccepted)
                {
                    return;
                }

                unitAccepted = true;

                ((IInternalIncident)calloutHolder.Callout).Units.Add(unit);
                calloutHolder.Callout.OnAccepted(unit);

                foreach (var player in unit.Players)
                {
                    player.GetUnderlyingPlayer().TriggerEvent(Events.Callout.OnAccepted, JsonConvert.SerializeObject(new ClientCallout
                    {
                        Id = calloutHolder.Callout.Id.ToString(),
                        Title = calloutHolder.Callout.Title,
                        Description = calloutHolder.Callout.Description,
                        Location = calloutHolder.Callout.Location,
                        Priority = calloutHolder.Callout.Priority,
                        ActivityRadius = calloutHolder.Callout.ActivityRadius,
                    }));
                }
            }

            var priorityCodeLabel = string.Empty;
            var priorityColorCode = string.Empty;
            switch (calloutHolder.Callout.Priority)
            {
                case FIncidentPriority.Code1:
                {
                    priorityCodeLabel = "LABEL.PRIORITY.CODE1";
                    priorityColorCode = "~b~";
                    break;
                }

                case FIncidentPriority.Code2:
                {
                    priorityCodeLabel = "LABEL.PRIORITY.CODE2";
                    priorityColorCode = "~y~";
                    break;
                }

                case FIncidentPriority.Code3:
                {
                    priorityCodeLabel = "LABEL.PRIORITY.CODE3";
                    priorityColorCode = "~r~";
                    break;
                }
            }

            foreach (var player in unit.Players)
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
                            Content = $"{calloutHolder.Callout.Title}~n~{{{{priority}}}}: {priorityColorCode}{{{{priorityCode}}}}",
                            ContentParams = new Dictionary<string, string>
                            {
                                ["priority"] = "LABEL.PRIORITY.SINGULAR",
                                ["priorityCode"] = priorityCodeLabel,
                            },
                        },
                    },
                    response =>
                    {
                        if (!response)
                        {
                            return;
                        }

                        OnAccepted();
                    });
            }
        }

        return filteredUnits;
    }
}