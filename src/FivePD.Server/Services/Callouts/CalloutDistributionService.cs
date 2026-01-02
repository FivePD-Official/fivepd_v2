using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Common.Extensions;
using FivePD.Common.Models;
using FivePD.Server.API;
using FivePD.Server.API.CalloutEvents;
using FivePD.Server.Attributes;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Interfaces.Dispatcher;
using FivePD.Server.Models;
using FivePD.Server.Repositories;
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
    private readonly IIncidentService _incidentService;
    private readonly IInternalDispatcherManagerService _dispatcherManagerService;
    private readonly CalloutDetailsRepository _calloutDetailsRepository;
    private readonly DepartmentAreaCoordinateRepository _departmentAreaCoordinateRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalloutDistributionService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="confirmationService">The <see cref="IConfirmationService"/> to use.</param>
    /// <param name="unitService">The <see cref="IUnitService"/> to use.</param>
    /// <param name="incidentService">The <see cref="IIncidentService"/> to use.</param>
    /// <param name="dispatcherManagerService">The <see cref="IDispatcherManagerService"/> to use.</param>
    /// <param name="calloutDetailsRepository">The <see cref="CalloutDetailsRepository"/> to use.</param>
    /// <param name="departmentAreaCoordinateRepository">The <see cref="DepartmentAreaCoordinateRepository"/> to use.</param>
    public CalloutDistributionService(
        ILogger logger,
        IConfigService configService,
        IConfirmationService confirmationService,
        IUnitService unitService,
        IIncidentService incidentService,
        IDispatcherManagerService dispatcherManagerService,
        CalloutDetailsRepository calloutDetailsRepository,
        DepartmentAreaCoordinateRepository departmentAreaCoordinateRepository)
    {
        this._logger = logger;
        this._configService = configService;
        this._confirmationService = confirmationService;
        this._unitService = unitService;
        this._incidentService = incidentService;
        this._dispatcherManagerService = (IInternalDispatcherManagerService)dispatcherManagerService;
        this._calloutDetailsRepository = calloutDetailsRepository;
        this._departmentAreaCoordinateRepository = departmentAreaCoordinateRepository;
    }

    /// <inheritdoc/>
    public event EventHandler<DispatchedCalloutEventArgs> OnCalloutDispatched;

    /// <inheritdoc/>
    public event EventHandler<AcceptedCalloutEventArgs> OnCalloutAccepted;

    /// <inheritdoc />
    public event EventHandler<UnitAssignedToCalloutEventArgs> OnUnitAssignedToCallout;

    /// <inheritdoc />
    public event EventHandler<UnitAssignedToCalloutEventArgs> OnUnitUnassignedFromCallout;

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
            this._dispatcherManagerService.SendIncidentToDispatchers(internalCallout.Callout);
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

        this._dispatcherManagerService.SendUpdateRequestToDispatchers();

        return true;
    }

    /// <inheritdoc />
    public bool UnassignUnitFromCallout(Unit unit, Callout callout, bool acceptedByUnit = false)
    {
        if (unit is null || callout is null)
        {
            this._logger.Warning("CalloutDistributionService.AssignUnitToCallout: unit or callout is null");
            return false;
        }

        if (!this._incidentService.UnassignUnitFromIncident(unit, callout))
        {
            return false;
        }

        callout.OnUnitUnassigned(unit);
        this.OnUnitUnassignedFromCallout?.Invoke(this, new UnitAssignedToCalloutEventArgs(callout, unit));

        this._dispatcherManagerService.SendUpdateRequestToDispatchers();

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

        var calloutDetails = this._calloutDetailsRepository.FindByTypeNameAndAssembly(
            internalCallout.Type.Name,
            internalCallout.Type.Assembly.GetName().Name,
            new CalloutDetailsRepository.CalloutDetailsLoadOptions { IncludeCalloutDetailsDepartments = true });
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
            var valueTuples = filteredUnits.Select(unit => (unit, unit.Players[0].Department())).ToList();
            foreach (var (_, department) in valueTuples)
            {
                department.AreaCoordinates = this._departmentAreaCoordinateRepository.GetAllByDepartmentId(department.Id).ToList();
            }

            filteredUnits = valueTuples
                .Where(x => x.Item2.AreaCoordinates.Count >= 3)
                .Where(x => internalCallout.Callout.Location.IsInsidePolygon(x.Item2.AreaCoordinates.Select(c => new Vector3(c.X, c.Y, 0f))))
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
                    Title = "LABEL.DISPATCH.911_CALLOUT",
                    Message = "LABEL.CALLOUT.CONTENT",
                    MessageParams = new Dictionary<string, string>
                    {
                        ["title"] = internalCallout.Callout.Title,
                        ["priority"] = internalCallout.Callout.Priority.GetLabelKey(),
                    },
                    PlayAudio = this._configService.GeneralConfig.Audio.Files.CalloutDispatched,
                    Keys = new Dictionary<string, string>
                    {
                        { "F1", "ACTION.ACCEPT.TITLE" },
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