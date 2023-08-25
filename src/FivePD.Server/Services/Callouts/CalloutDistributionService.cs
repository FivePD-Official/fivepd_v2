using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.API;
using FivePD.Server.API.CalloutEvents;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Models;
using Newtonsoft.Json;

namespace FivePD.Server.Services.Callouts;

#pragma warning disable CS4014
#pragma warning disable S3241

/// <inheritdoc cref="ICalloutDistributionService" />
public class CalloutDistributionService : ICalloutDistributionService, IInternalCalloutDistributionService
{
    private readonly DatabaseContext _databaseContext;
    private readonly IConfigService _configService;
    private readonly IConfirmationService _confirmationService;
    private readonly IUnitService _unitService;
    private readonly PlayerList _playerList;
    private readonly ICalloutStoreService _calloutStoreService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalloutDistributionService"/> class.
    /// </summary>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="confirmationService">The <see cref="IConfirmationService"/> to use.</param>
    /// <param name="unitService">The <see cref="IUnitService"/> to use.</param>
    /// <param name="playerList">The <see cref="PlayerList"/> to use.</param>
    /// <param name="calloutStoreService">The <see cref="ICalloutStoreService"/> to use.</param>
    public CalloutDistributionService(
        DatabaseContext databaseContext,
        IConfigService configService,
        IConfirmationService confirmationService,
        IUnitService unitService,
        PlayerList playerList,
        ICalloutStoreService calloutStoreService)
    {
        this._databaseContext = databaseContext;
        this._configService = configService;
        this._confirmationService = confirmationService;
        this._unitService = unitService;
        this._playerList = playerList;
        this._calloutStoreService = calloutStoreService;
    }

    /// <inheritdoc/>
    public event EventHandler<DispatchedCalloutEventArgs> OnCalloutDispatched;

    /// <inheritdoc/>
    public event EventHandler<AcceptedCalloutEventArgs> OnCalloutAccepted;

    /// <inheritdoc />
    public event EventHandler<UnitAssignedToCalloutEventArgs> OnUnitAssignedToCallout;

    /// <inheritdoc/>
    void IInternalCalloutDistributionService.DistributeCallout(CalloutHolder calloutHolder)
    {
        if (this._configService.GeneralConfig.Callout.DispatchTypeAuto())
        {
            var notifiedUnits = this.DispatchCalloutToUnits(calloutHolder);
            if (notifiedUnits.Count > 0)
            {
                this.OnCalloutDispatched?.Invoke(this, new DispatchedCalloutEventArgs(calloutHolder.Callout, notifiedUnits));
                calloutHolder.Callout.OnDispatched(notifiedUnits);
            }
        }
        else
        {
            var sentToAnyPlayer = false;
            foreach (var player in this._playerList)
            {
                var fPlayer = FPlayer.FromPlayer(player);
                if (!fPlayer.Dispatcher.Value)
                {
                    continue;
                }

                sentToAnyPlayer = true;
                player.TriggerEvent(Events.Dispatch.SendCalloutToDispatcher, JsonConvert.SerializeObject(calloutHolder.Callout.ToCalloutForDispatcherDTO()));
            }

            if (!sentToAnyPlayer)
            {
                ((IInternalCalloutStoreService)this._calloutStoreService).ActiveCallouts.RemoveAll(x => x.Callout == calloutHolder.Callout);
            }
        }
    }

    /// <inheritdoc />
    public void AssignUnitToCallout(Unit unit, Callout callout, bool acceptedByUnit = false)
    {
        if (callout.Units.Contains(unit))
        {
            return;
        }

        ((IInternalIncident)callout).Units.Add(unit);

        unit.Availability.Value = FUnitAvailability.NotReceiving;
        unit.Callout = new SimpleCallout(callout);
        ((IInternalUnit)unit).InternalCallout = callout;

        if (acceptedByUnit)
        {
            callout.OnAccepted(unit);
            this.OnCalloutAccepted?.Invoke(this, new AcceptedCalloutEventArgs(callout, unit));
        }
        else
        {
            callout.OnUnitAssigned(unit);
            this.OnUnitAssignedToCallout?.Invoke(this, new UnitAssignedToCalloutEventArgs(callout, unit));
        }

        foreach (var player in unit.Players)
        {
            player.GetUnderlyingPlayer().TriggerEvent(Events.Callout.OnAccepted, JsonConvert.SerializeObject(new ClientCallout
            {
                Id = callout.Id.ToString(),
                Title = callout.Title,
                Description = callout.Description,
                Location = callout.Location,
                Priority = callout.Priority,
                ActivityRadius = callout.ActivityRadius,
            }));
        }
    }

    private List<Unit> DispatchCalloutToUnits(CalloutHolder calloutHolder)
    {
        var filteredUnits = this._unitService.Units()
            .Where(unit => this._calloutStoreService.ActiveCallouts.All(callout => callout.Units.All(u => u.Callsign != unit.Callsign)))
            .Where(unit => unit.Availability.Value == FUnitAvailability.Receiving);
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

                this.AssignUnitToCallout(unit, calloutHolder.Callout, true);
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

        return notifiedUnits;
    }
}