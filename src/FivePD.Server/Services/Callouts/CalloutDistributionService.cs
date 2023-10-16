using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services.Callouts;

#pragma warning disable CS4014
#pragma warning disable S3241

/// <inheritdoc cref="ICalloutDistributionService" />
public class CalloutDistributionService : ICalloutDistributionService, IInternalCalloutDistributionService
{
    private readonly ILogger _logger;
    private readonly IConfigService _configService;
    private readonly IConfirmationService _confirmationService;
    private readonly IUnitService _unitService;
    private readonly PlayerList _playerList;
    private readonly IInternalIncidentService _incidentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalloutDistributionService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="confirmationService">The <see cref="IConfirmationService"/> to use.</param>
    /// <param name="unitService">The <see cref="IUnitService"/> to use.</param>
    /// <param name="playerList">The <see cref="PlayerList"/> to use.</param>
    /// <param name="incidentService">The <see cref="IIncidentService"/> to use.</param>
    public CalloutDistributionService(
        ILogger logger,
        IConfigService configService,
        IConfirmationService confirmationService,
        IUnitService unitService,
        PlayerList playerList,
        IIncidentService incidentService)
    {
        this._logger = logger;
        this._configService = configService;
        this._confirmationService = confirmationService;
        this._unitService = unitService;
        this._playerList = playerList;
        this._incidentService = (IInternalIncidentService)incidentService;
    }

    /// <inheritdoc/>
    public event EventHandler<DispatchedCalloutEventArgs> OnCalloutDispatched;

    /// <inheritdoc/>
    public event EventHandler<AcceptedCalloutEventArgs> OnCalloutAccepted;

    /// <inheritdoc />
    public event EventHandler<UnitAssignedToCalloutEventArgs> OnUnitAssignedToCallout;

    /// <inheritdoc/>
    void IInternalCalloutDistributionService.DistributeCallout(InternalCallout internalCallout, bool manualTrigger)
    {
        if (internalCallout is null)
        {
            this._logger.Warning("CalloutDistributionService.DistributeCallout: internalCallout is null");
            return;
        }

        if (this._configService.GeneralConfig.Callout.DispatchTypeAuto() || manualTrigger)
        {
            var notifiedUnits = this.DispatchCalloutToUnits(internalCallout);
            if (notifiedUnits.Count > 0)
            {
                this.OnCalloutDispatched?.Invoke(this, new DispatchedCalloutEventArgs(internalCallout.Callout, notifiedUnits));
                internalCallout.Callout.OnDispatched(notifiedUnits);
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
                player.TriggerEvent(Events.Dispatch.SendCalloutToDispatcher, JsonConvert.SerializeObject(internalCallout.Callout.ToCalloutForDispatcherDTO()));
            }

            if (!sentToAnyPlayer)
            {
                this._incidentService.InternalCallouts.RemoveAll(x => x.Callout == internalCallout.Callout);
            }
        }
    }

    /// <inheritdoc />
    public bool AssignUnitToCallout(Unit unit, Callout callout, bool acceptedByUnit = false)
    {
        if (unit is null || callout is null)
        {
            this._logger.Warning("CalloutDistributionService.AssignUnitToCallout: unit or callout is null");
            return false;
        }

        if (!this._incidentService.AssignUnitToIncident(unit, callout))
        {
            return false;
        }

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

        return true;
    }

    private List<Unit> DispatchCalloutToUnits(InternalCallout internalCallout)
    {
        if (internalCallout is null)
        {
            this._logger.Warning("CalloutDistributionService.DispatchCalloutToUnits: internalCallout is null");
            return new List<Unit>();
        }

        var filteredUnits = this._unitService.Units()
            .Where(unit => this._incidentService.ActiveCallouts.All(callout => callout.Units.All(u => u.Callsign != unit.Callsign)))
            .Where(unit => unit.Availability.Value == FUnitAvailability.Receiving)
            .ToList();

        if (filteredUnits.Select(x => x.Players.Count).Aggregate(0, (acc, count) => acc + count) < internalCallout.Properties.MinOnDutyOfficers)
        {
            return new List<Unit>();
        }

        if (this._configService.GeneralConfig.Callout.DynamicDispatch.Enabled)
        {
            filteredUnits = filteredUnits.Where(unit =>
                unit.Players.Any(player => player.GetUnderlyingPlayer().Character.Position.DistanceToSquared(internalCallout.Callout.Location) <=
                                           this._configService.GeneralConfig.Callout.DynamicDispatch.Radius))
                .ToList();
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

                this.AssignUnitToCallout(unit, internalCallout.Callout, true);
            }

            var priorityCodeLabel = string.Empty;
            var priorityColorCode = string.Empty;
            switch (internalCallout.Callout.Priority)
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
                            Content = $"{internalCallout.Callout.Title}~n~{{{{priority}}}}: {priorityColorCode}{{{{priorityCode}}}}",
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

    [UIEvent(Events.Callout.SelfAssign, true)]
    private void SelfAssignUI([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<StringIdRequest>();

        var callout = this._incidentService.ActiveCallouts.FirstOrDefault(x => x.Id == Guid.Parse(request.Id));
        if (callout is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var fPlayer = FPlayer.FromPlayer(player);
        if (fPlayer.Unit is null)
        {
            return;
        }

        var assignedToCallout = this.AssignUnitToCallout(fPlayer.Unit, callout);
        networkCallbackDelegate.InvokeWithResponse(assignedToCallout, "UI.LABEL.SUCCESS.SELF_ASSIGN", "UI.LABEL.INCIDENT.SUCCESS.SELF_ASSIGN");
    }
}