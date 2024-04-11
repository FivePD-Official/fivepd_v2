using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Extensions;
using FivePD.Common.Models;
using FivePD.Common.Utilities;
using FivePD.Server.API;
using FivePD.Server.API.CalloutEvents;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services.Callouts;

#pragma warning disable CS4014
#pragma warning disable S3241

/// <inheritdoc cref="ICalloutDistributionService" />
[Obfuscate]
public class CalloutDistributionService : ICalloutDistributionService, IInternalCalloutDistributionService
{
    private readonly ILogger _logger;
    private readonly IConfigService _configService;
    private readonly IConfirmationService _confirmationService;
    private readonly IUnitService _unitService;
    private readonly PlayerList _playerList;
    private readonly IInternalIncidentService _incidentService;
    private readonly DatabaseContext _databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalloutDistributionService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="confirmationService">The <see cref="IConfirmationService"/> to use.</param>
    /// <param name="unitService">The <see cref="IUnitService"/> to use.</param>
    /// <param name="playerList">The <see cref="PlayerList"/> to use.</param>
    /// <param name="incidentService">The <see cref="IIncidentService"/> to use.</param>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    public CalloutDistributionService(
        ILogger logger,
        IConfigService configService,
        IConfirmationService confirmationService,
        IUnitService unitService,
        PlayerList playerList,
        IIncidentService incidentService,
        DatabaseContext databaseContext)
    {
        this._logger = logger;
        this._configService = configService;
        this._confirmationService = confirmationService;
        this._unitService = unitService;
        this._playerList = playerList;
        this._incidentService = (IInternalIncidentService)incidentService;
        this._databaseContext = databaseContext;
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

    // TODO: move to a better place
    private bool IsPointInsidePolygon(IReadOnlyList<Vector3> points, Vector3 point)
    {
        var count = 0;
        var n = points.Count;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if (points[i].Y > point.Y != points[j].Y > point.Y &&
                point.X < ((points[j].X - points[i].X) * (point.Y - points[i].Y) / (points[j].Y - points[i].Y)) + points[i].X)
            {
                count++;
            }
        }

        return count % 2 == 1;
    }

    private List<Unit> DispatchCalloutToUnits(InternalCallout internalCallout)
    {
        if (internalCallout is null)
        {
            this._logger.Warning("CalloutDistributionService.DispatchCalloutToUnits: internalCallout is null");
            return new List<Unit>();
        }

        var filteredUnits = this._unitService.Units()
            .Where(unit => ((IIncidentService)this._incidentService).ActiveCallouts.All(callout => callout.Units.All(u => u.Callsign != unit.Callsign)))
            .Where(unit => unit.Availability.Value == FUnitAvailability.Receiving)
            .ToList();

        var calloutDetails = this._databaseContext.CalloutDetails
            .Include(x => x.CalloutDetailsDepartments)
            .FirstOrDefault(x => x.TypeName == internalCallout.Type.Name && x.AssemblyName == internalCallout.Type.Assembly.GetName().Name);
        if (calloutDetails is not null)
        {
            filteredUnits = calloutDetails.RulesetType == CalloutDetailsRulesetType.Allow
                ? filteredUnits.Where(unit => calloutDetails.CalloutDetailsDepartments.Any(department => department.DepartmentId == unit.DepartmentId)).ToList()
                : filteredUnits.Where(unit => calloutDetails.CalloutDetailsDepartments.All(department => department.DepartmentId != unit.DepartmentId))
                    .ToList();
        }
        else
        {
            this._logger.Warning(
                $"CalloutDistributionService.DispatchCalloutToUnits: calloutDetails is null for {internalCallout.Type.Name} - this should never happen");
        }

        if (filteredUnits.Select(x => x.Players.Count).Aggregate(0, (acc, count) => acc + count) < internalCallout.Properties.MinOnDutyOfficers)
        {
            return new List<Unit>();
        }

        if (this._configService.GeneralConfig.Callout.AreaBasedDispatch)
        {
            var valueTuples = filteredUnits.Select(unit => (unit, unit.Players[0].Department)).ToList();
            foreach (var (_, department) in valueTuples)
            {
                this._databaseContext
                    .Entry(department)
                    .Collection(x => x.AreaCoordinates)
                    .Load();
            }

            filteredUnits = valueTuples
                .Where(x => x.Department.AreaCoordinates.Count >= 3)
                .Where(x => this.IsPointInsidePolygon(
                    x.Department.AreaCoordinates.Select(c => new Vector3(c.X, c.Y, 0f)).ToList(),
                    internalCallout.Callout.Location))
                .Select(x => x.unit)
                .ToList();
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

            void Accepted()
            {
                if (unitAccepted)
                {
                    return;
                }

                unitAccepted = true;

                this.AssignUnitToCallout(unit, internalCallout.Callout, true);
            }

            this._confirmationService.TriggerConfirmation(
                unit,
                new ConfirmationProperties
                {
                    Notification = new ConfirmationProperties.NotificationProperties
                    {
                        Texture = "CHAR_CALL911",
                        Sender = "LABEL.DISPATCH",
                        Subject = "LABEL.CALLOUT.TITLE", // TODO: check if this is new GtaStyleText().Yellow("LABEL.CALLOUT.TITLE").Build() and the title value is "Callout" without color, the text is correctly translated on the client
                        Content = "LABEL.CALLOUT.CONTENT",
                        ContentParams = new Dictionary<string, string>
                        {
                            ["title"] = new GtaStyleText()
                                .Append(internalCallout.Callout.Title)
                                .NewLine()
                                .Build(),
                            ["priority"] = internalCallout.Callout.Priority.GetLabelKey(),
                        },
                    },
                },
                response =>
                {
                    if (!response)
                    {
                        return;
                    }

                    Accepted();
                });
        }

        return notifiedUnits;
    }
}