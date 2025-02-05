using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.API;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc />
[Obfuscate]
public class IncidentService : IIncidentService, IInternalIncidentService
{
    private readonly ILogger _logger;
    private readonly IInternalIncidentHistoryService _incidentHistoryService;
    private readonly IProgressionSystemService _progressionSystemService;
    private readonly IConfigService _configService;
    private readonly IVehicleStorageService _vehicleStorageService;

    private readonly List<TrafficStop> _trafficStops = new();

    private readonly List<InternalCallout> _callouts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="progressionSystemService">The <see cref="IProgressionSystemService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="incidentHistoryService">The <see cref="IIncidentHistoryService"/> to use.</param>
    /// <param name="vehicleStorageService">The <see cref="IVehicleStorageService"/> to use.</param>
    public IncidentService(
        ILogger logger,
        IProgressionSystemService progressionSystemService,
        IConfigService configService,
        IIncidentHistoryService incidentHistoryService,
        IVehicleStorageService vehicleStorageService)
    {
        this._logger = logger;
        this._incidentHistoryService = (IInternalIncidentHistoryService)incidentHistoryService;
        this._progressionSystemService = progressionSystemService;
        this._configService = configService;
        this._vehicleStorageService = vehicleStorageService;
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
        if (!type.IsSubclassOf(typeof(Callout)) && type != typeof(TrafficStop))
        {
            return false;
        }

        if (incident.Units.Contains(unit))
        {
            return false;
        }

        var incidentType = type.IsSubclassOf(typeof(Callout)) ? FIncidentType.Callout : FIncidentType.TrafficStop;
        this._incidentHistoryService.AddUnitToIncidentHistory(incident, incidentType, unit);

        // Remove the unit from the previous incident
        if (unit.Callout is not null)
        {
            ((IInternalIncident)unit.InternalCallout).Units.Remove(unit);
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
                unit.InternalCallout = (Callout)incident;

                // Settings this is not needed for traffic stops
                unit.Availability.Value = FUnitAvailability.NotReceiving;

                break;
            case FIncidentType.TrafficStop:
                unit.TrafficStop = (TrafficStop)incident;

                unit.Callout = null;
                unit.InternalCallout = null;

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
                TrafficStopInvolvedVehicle = incidentType == FIncidentType.Callout ? 0 : ((TrafficStop)incident).Vehicle.NetworkId,
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

        var callout = player.Unit?.InternalCallout;
        if (callout is not null)
        {
            return callout;
        }

        var trafficStop = player.Unit?.TrafficStop;
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
        if (!type.IsSubclassOf(typeof(Callout)) && type != typeof(TrafficStop))
        {
            return;
        }

        var incidentType = type.IsSubclassOf(typeof(Callout)) ? FIncidentType.Callout : FIncidentType.TrafficStop;
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
                var fVehicle = this._vehicleStorageService.TryGetVehicle(((TrafficStop)incident).Vehicle.NetworkId);
                if (fVehicle is not null)
                {
                    fVehicle.Stopped.Value = false;
                }

                break;
        }

        foreach (var unit in incident.Units)
        {
            unit.Availability.Value = FUnitAvailability.Receiving;
            switch (incidentType)
            {
                case FIncidentType.Callout:
                    unit.Callout = null;
                    unit.InternalCallout = null;
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
    
    [UIEvent(Events.Incident.Get)]
    private void GetUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<StringIdRequest>();
        if (!Guid.TryParse(request.Id, out var id))
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var found = false;
        IncidentDTOs.IncidentToClientDTO incident = default;
        var callout = this._callouts.FirstOrDefault(x => x.Callout.Id == id);
        if (callout is not null)
        {
            found = true;
            incident = new IncidentDTOs.IncidentToClientDTO
            {
                Id = callout.Callout.Id.ToString(),
                Type = FIncidentType.Callout,
                Title = callout.Callout.Title,
                Description = callout.Callout.Description,
                Location = callout.Callout.Location,
                Units = callout.Callout.Units.Select(p => p.Callsign).ToList(),
            };
        }
        
        var trafficStops = this._trafficStops.FirstOrDefault(x => x.Id == id);
        if (trafficStops is not null)
        {
            found = true;
            incident =  new IncidentDTOs.IncidentToClientDTO
            {
                Id = trafficStops.Id.ToString(),
                Type = FIncidentType.TrafficStop,
                Title = trafficStops.Title,
                Description = trafficStops.Description,
                Location = trafficStops.Location,
                Units = trafficStops.Units.Select(p => p.Callsign).ToList(),
            };
        }

        if (!found)
        {
            return;
        }

        networkCallbackDelegate.InvokeWithJson(incident);
    }

    [UIEvent(Events.Incident.GetAllActive)]
    private void GetAllActiveUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var activeCallouts = this._callouts
            .Where(x => x.Callout.Units.Count > 0)
            .Select(x => new IncidentDTOs.IncidentToClientDTO
            {
                Id = x.Callout.Id.ToString(),
                Type = FIncidentType.Callout,
                Title = x.Callout.Title,
                Description = x.Callout.Description,
                Location = x.Callout.Location,
                Units = x.Callout.Units.Select(p => p.Callsign).ToList(),
            })
            .ToList();
        var trafficStops = this._trafficStops
            .Select(x => new IncidentDTOs.IncidentToClientDTO
            {
                Id = x.Id.ToString(),
                Type = FIncidentType.TrafficStop,
                Title = x.Title,
                Description = x.Description,
                Location = x.Location,
                Units = x.Units.Select(p => p.Callsign).ToList(),
            })
            .ToList();

        var incidents = activeCallouts.Concat(trafficStops).ToList();
        var page = new Page<IncidentDTOs.IncidentToClientDTO>
        {
            Items = incidents,
            CurrentPage = 1,
            ItemCount = incidents.Count,
            PageCount = 1,
            PageSize = incidents.Count,
        };
        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.Incident.SelfAssign, true)]
    private void SelfAssignUI([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<StringIdRequest>();

        IIncident incident = null;
        var callout = this._callouts.FirstOrDefault(x => x.Callout.Id == Guid.Parse(request.Id));
        if (callout is not null)
        {
            incident = callout.Callout;
        }
        else
        {
            var trafficStop = this._trafficStops.FirstOrDefault(x => x.Id == Guid.Parse(request.Id));
            if (trafficStop is not null)
            {
                incident = trafficStop;
            }
        }

        if (incident is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var fPlayer = FPlayer.FromPlayer(player);
        if (fPlayer.Unit is null)
        {
            return;
        }

        var assignedToCallout = ((IInternalIncidentService)this).AssignUnitToIncident(fPlayer.Unit, incident);
        networkCallbackDelegate.InvokeWithResponse(assignedToCallout, "UI.LABEL.SUCCESS.SELF_ASSIGN", "UI.LABEL.INCIDENT.SUCCESS.SELF_ASSIGN");
    }
}