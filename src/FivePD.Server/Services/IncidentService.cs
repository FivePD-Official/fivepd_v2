using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.API;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Models;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class IncidentService : IInternalIncidentService
{
    private readonly ILogger _logger;
    private readonly IInternalIncidentHistoryService _incidentHistoryService;
    private readonly IProgressionSystemService _progressionSystemService;
    private readonly IConfigService _configService;

    private readonly List<TrafficStop> _trafficStops = new();

    private readonly List<InternalCallout> _callouts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="progressionSystemService">The <see cref="IProgressionSystemService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="incidentHistoryService">The <see cref="IIncidentHistoryService"/> to use.</param>
    public IncidentService(
        ILogger logger,
        IProgressionSystemService progressionSystemService,
        IConfigService configService,
        IIncidentHistoryService incidentHistoryService)
    {
        this._logger = logger;
        this._incidentHistoryService = (IInternalIncidentHistoryService)incidentHistoryService;
        this._progressionSystemService = progressionSystemService;
        this._configService = configService;
    }

    /// <inheritdoc/>
    public event EventHandler<Callout> OnCalloutCompleted;

    /// <inheritdoc />
    public ReadOnlyCollection<Callout> ActiveCallouts => this._callouts.Select(x => x.Callout).ToList().AsReadOnly();

    /// <inheritdoc />
    public ReadOnlyCollection<TrafficStop> TrafficStops => this._trafficStops.AsReadOnly();

    /// <inheritdoc/>
    List<InternalCallout> IInternalIncidentService.InternalCallouts => this._callouts;

    /// <inheritdoc/>
    List<TrafficStop> IInternalIncidentService.InternalTrafficStops => this._trafficStops;

    /// <inheritdoc/>
    bool IInternalIncidentService.AssignUnitToIncident(Unit unit, IIncident incident)
    {
        if (unit is null || incident is null)
        {
            this._logger.Warning("IncidentService.AssignUnitToIncident: unit or incident is null");
            return false;
        }

        var type = incident.GetType();
        if (type != typeof(Callout) && type != typeof(TrafficStop))
        {
            return false;
        }

        if (incident.Units.Contains(unit))
        {
            return false;
        }

        var incidentType = type == typeof(Callout) ? FIncidentType.Callout : FIncidentType.TrafficStop;
        this._incidentHistoryService.AddUnitToIncidentHistory(incident, incidentType, unit);

        // Remove the unit from the previous incident
        if (unit.Callout is not null)
        {
            ((IInternalIncident)((IInternalUnit)unit).InternalCallout).Units.Remove(unit);
        }

        if (unit.TrafficStop is not null)
        {
            ((IInternalIncident)unit.TrafficStop).Units.Remove(unit);
        }

        // Add the unit to the new incident
        ((IInternalIncident)incident).Units.Add(unit);
        switch (incidentType)
        {
            case FIncidentType.Callout:
                unit.TrafficStop = null;

                unit.Callout = new SimpleCallout((Callout)incident);
                ((IInternalUnit)unit).InternalCallout = (Callout)incident;

                // Settings this is not needed for traffic stops
                unit.Availability.Value = FUnitAvailability.NotReceiving;

                break;
            case FIncidentType.TrafficStop:
                unit.TrafficStop = (TrafficStop)incident;

                unit.Callout = null;
                ((IInternalUnit)unit).InternalCallout = null;

                break;
        }

        foreach (var player in unit.Players)
        {
            player.GetUnderlyingPlayer().TriggerEvent(Events.Incident.OnAssigned, JsonConvert.SerializeObject(new ClientIncidentDTO
            {
                Id = incident.Id.ToString(),
                Type = incidentType,
                Title = incident.Title,
                Description = incident.Description,
                Location = incident.Location,
                Priority = incident.Priority,
                ActivityRadius = incidentType == FIncidentType.Callout ? ((Callout)incident).ActivityRadius : 0f,
            }));
        }

        return true;
    }

    /// <inheritdoc />
    public IIncident GetCurrentIncidentOfPlayer(FPlayer player)
    {
        if (player is null)
        {
            this._logger.Warning("IncidentService.GetCurrentIncidentOfPlayer: player is null");
            return null;
        }

        var callout = this._callouts.FirstOrDefault(x => x.Callout.Units.Any(unit => unit.Players.Any(p => p.NetId == player.NetId)));
        if (callout is not null)
        {
            return callout.Callout;
        }

        var trafficStop = this._trafficStops.FirstOrDefault(x => x.Units.Any(unit => unit.Players.Any(p => p.NetId == player.NetId)));
        return trafficStop;
    }

    /// <inheritdoc />
    public void CompleteIncidentOfPlayer(FPlayer player)
    {
        this.CompleteIncident(this.GetCurrentIncidentOfPlayer(player));
    }

    /// <inheritdoc />
    public void CompleteIncident(IIncident incident)
    {
        if (incident is null)
        {
            return;
        }

        var type = incident.GetType();
        if (type != typeof(Callout) && type != typeof(TrafficStop))
        {
            return;
        }

        var incidentType = type == typeof(Callout) ? FIncidentType.Callout : FIncidentType.TrafficStop;
        var xpToAdd = 0;
        switch (incidentType)
        {
            case FIncidentType.Callout:
                xpToAdd = this._configService.GeneralConfig.ProgressionSystem.ExperiencePoints.CalloutCompleted;
                ((Callout)incident).OnCompleted();
                this.OnCalloutCompleted?.Invoke(this, (Callout)incident);
                this._callouts.RemoveAll(x => x.Callout.Id == incident.Id);
                break;
            case FIncidentType.TrafficStop:
                xpToAdd = this._configService.GeneralConfig.ProgressionSystem.ExperiencePoints.TrafficStopCompleted;
                this._trafficStops.RemoveAll(x => x.Id == incident.Id);
                break;
        }

        foreach (var unit in incident.Units)
        {
            unit.Availability.Value = FUnitAvailability.Receiving;
            switch (incidentType)
            {
                case FIncidentType.Callout:
                    unit.Callout = null;
                    ((IInternalUnit)unit).InternalCallout = null;
                    break;
                case FIncidentType.TrafficStop:
                    unit.TrafficStop = null;
                    break;
            }

            this._progressionSystemService.AddExperiencePoints(unit, xpToAdd);
            foreach (var player in unit.Players)
            {
                player.GetUnderlyingPlayer().TriggerEvent(Events.Incident.OnCompleted);
            }
        }
    }
}