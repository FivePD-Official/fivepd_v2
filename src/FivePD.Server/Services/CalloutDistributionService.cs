using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.API;
using FivePD.Server.API.CalloutEvents;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using Newtonsoft.Json;
using Serilog;
using Debug = CitizenFX.Core.Debug;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.ICalloutDistributionService" />
internal class CalloutDistributionService : ICalloutDistributionService, IServiceWithEvents
{
    private readonly ITickQueueService _tickQueueService;
    private readonly IConfigService _configService;
    private readonly IRandomService _randomService;
    private readonly IConfirmationService _confirmationService;
    private readonly ILogger _logger;
    private readonly IInternalCalloutManagerService _calloutManagerService;
    private readonly IUnitService _unitService;
    private readonly IDispatchService _dispatchService;

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
    /// <param name="dispatchService">The <see cref="IDispatchService"/> to use.</param>
    public CalloutDistributionService(
        ITickQueueService tickQueueService,
        IConfigService configService,
        IRandomService randomService,
        ILogger logger,
        IConfirmationService confirmationService,
        IInternalCalloutManagerService calloutManagerService,
        IUnitService unitService,
        IDispatchService dispatchService)
    {
        this._tickQueueService = tickQueueService;
        this._configService = configService;
        this._randomService = randomService;
        this._logger = logger;
        this._confirmationService = confirmationService;
        this._calloutManagerService = calloutManagerService;
        this._unitService = unitService;
        this._dispatchService = dispatchService;
    }

    /// <inheritdoc/>
    public event EventHandler<DispatchedCallout> OnCalloutDispatched;

    /// <inheritdoc/>
    public event EventHandler<AcceptedCallout> OnCalloutAccepted;

    /// <inheritdoc/>
    public void RegisterEvents()
    {
        this._nextCalloutInMs = this._randomService.Random.Next(
            this._configService.GeneralConfig.Callout.MinIntervalInMs,
            this._configService.GeneralConfig.Callout.MaxIntervalInMs);
        this._nextCalloutTimer.Start();
        this._tickQueueService.EnqueueInterval(this.GenerateCalloutAsync, 1000);
    }

    private async Task GenerateCalloutAsync()
    {
        if (this._configService.GeneralConfig.Callout.MaxActiveCallouts > 0 &&
            this._calloutManagerService.ActiveCallouts.Count >= this._configService.GeneralConfig.Callout.MaxActiveCallouts)
        {
            return;
        }

        if (this._nextCalloutTimer.ElapsedMilliseconds < this._nextCalloutInMs)
        {
            return;
        }

        this._nextCalloutInMs = this._randomService.Random.Next(
            this._configService.GeneralConfig.Callout.MinIntervalInMs,
            this._configService.GeneralConfig.Callout.MaxIntervalInMs);
        this._nextCalloutTimer.Restart();

        var calloutTypes = this._calloutManagerService.CalloutTypes();
        var calloutType = calloutTypes[this._randomService.Random.Next(0, calloutTypes.Count)];
        await this.DispatchCalloutAsync(calloutType);
    }

    private async Task DispatchCalloutAsync(Type type, bool invokeCanBeDispatch = true)
    {
        var callout = (Callout)ServiceLocator.GetService(type);
        if (callout is null)
        {
            this._logger.Warning("A callout with the type of \"{Type}\" could not be instantiated.", type);
            return;
        }

        var properties = await callout.Setup();
        callout.Id = Guid.NewGuid();
        callout.Title = properties.Title;
        callout.Description = properties.Description;
        callout.Priority = properties.Priority;
        callout.ActivityRadius = properties.ActivityRadius;
        callout.GeneratedAt = DateTime.Now;

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

        if (invokeCanBeDispatch)
        {
            var canBeDispatched = await callout.CanBeDispatched();
            if (!canBeDispatched)
            {
                return;
            }
        }

        var calloutHolder = new CalloutHolder(callout, new Stopwatch());
        this._calloutManagerService.ActiveCallouts.Add(calloutHolder);
        calloutHolder.Timer.Start();

        if (this._configService.GeneralConfig.Callout.DispatchTypeAuto())
        {
            var notifiedUnits = this.DispatchCalloutToUnits(calloutHolder);
            this.OnCalloutDispatched?.Invoke(this, new DispatchedCallout
            {
                Callout = callout,
                NotifiedUnits = notifiedUnits,
            });
            await callout.OnDispatched(notifiedUnits);
        }
        else
        {
            this._dispatchService.SendCalloutToDispatchers(calloutHolder.Callout);
        }
    }

    private List<Unit> DispatchCalloutToUnits(CalloutHolder calloutHolder)
    {
        var filteredUnits = this._unitService.Units()
            .Where(unit => this._calloutManagerService.ActiveCallouts.All(holder => holder.Callout.Units.All(u => u.Callsign != unit.Callsign)))
            .Where(unit => unit.Players.All(p => p.Availability.Value == FPlayerAvailability.Receiving));
        if (this._configService.GeneralConfig.Callout.DynamicDispatch.Enabled)
        {
            filteredUnits = filteredUnits.Where(unit =>
                unit.Players.Any(player => player.GetUnderlyingPlayer().Character.Position.DistanceToSquared2D(calloutHolder.Callout.Location) <=
                                           this._configService.GeneralConfig.Callout.DynamicDispatch.Radius));
        }

        var notifiedUnits = filteredUnits.ToList();
        foreach (var unit in notifiedUnits)
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
                this.OnCalloutAccepted?.Invoke(this, new AcceptedCallout
                {
                    Callout = calloutHolder.Callout,
                    Unit = unit,
                });

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

                        player.Availability.Value = FPlayerAvailability.NotReceiving;
                        OnAccepted();
                    });
            }
        }

        return notifiedUnits;
    }

    [UIEvent(Events.UI.Callout.Dispatch)]
    private void DispatchCalloutUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<DispatchCalloutRequest>();
        var calloutType = this._calloutManagerService.CalloutTypes().FirstOrDefault(x => x.Name == request.TypeName);
        if (calloutType is null)
        {
            networkCallbackDelegate.Invoke(null);
            return;
        }

#pragma warning disable CS4014
        this.DispatchCalloutAsync(calloutType, false);
#pragma warning restore CS4014

        networkCallbackDelegate.Invoke(null);
    }
}