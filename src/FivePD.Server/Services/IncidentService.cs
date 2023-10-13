using FivePD.Common.Enums;
using FivePD.Server.API;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

#pragma warning disable

public class IncidentService : IIncidentService
{
    private readonly IInternalIncidentHistoryService _incidentHistoryService;

    public IncidentService(IIncidentHistoryService incidentHistoryService)
    {
        this._incidentHistoryService = (IInternalIncidentHistoryService)incidentHistoryService;
    }
    
    public bool AssignUnitToIncident(Unit unit, IIncident incident)
    {
        var type = incident.GetType();
        if (type != typeof(Callout) || type != typeof(TrafficStop))
        {
            return false;
        }
        
        if (incident.Units.Contains(unit))
        {
            return false;
        }

        var incidentType = type == typeof(Callout) ? FIncidentType.Callout : FIncidentType.TrafficStop;
        this._incidentHistoryService.AddUnitToIncidentHistory(incident, incidentType, unit);

        ((IInternalIncident)incident).Units.Add(unit);
        
        unit.Availability.Value = FUnitAvailability.NotReceiving;

        switch (incidentType)
        {
            case FIncidentType.Callout:
                unit.TrafficStop = null;

                unit.Callout = new SimpleCallout((Callout)incident);
                ((IInternalUnit)unit).InternalCallout = (Callout)incident;
                break;
            case FIncidentType.TrafficStop:
                unit.Callout = null;
                ((IInternalUnit)unit).InternalCallout = null;

                unit.TrafficStop = (TrafficStop)incident;
                break;
        }

        foreach (var player in unit.Players)
        {
            // player.GetUnderlyingPlayer().TriggerEvent(Events.Callout.OnAccepted, JsonConvert.SerializeObject(new ClientCallout
            // {
            //     Id = callout.Id.ToString(),
            //     Title = callout.Title,
            //     Description = callout.Description,
            //     Location = callout.Location,
            //     Priority = callout.Priority,
            //     ActivityRadius = callout.ActivityRadius,
            // }));
        }

        return true;
    }
}