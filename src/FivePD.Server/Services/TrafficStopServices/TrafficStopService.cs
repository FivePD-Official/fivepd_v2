using System;
using System.Collections.Generic;
using FivePD.Common.Enums;
using FivePD.Server.API;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.TrafficStopInterfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Services.TrafficStopServices;

#pragma warning disable

public class TrafficStopService : ITrafficStopService
{
    private readonly IIncidentService _incidentService;
    private readonly List<TrafficStop> _trafficStops;

    public TrafficStopService(IIncidentService incidentService)
    {
        this._incidentService = incidentService;
    }
    
    public TrafficStop GenerateTrafficStop(Unit unit, FVehicle vehicle)
    {
        var trafficStop = new TrafficStop
        {
            Id = Guid.NewGuid(),
            Title = "Traffic stop",
            Description = "",
            GeneratedAt = DateTime.Now,
            Location = vehicle.GetUnderlyingVehicle.Position,
            Priority = FIncidentPriority.Code1,
            Vehicle = vehicle,
        };
        
        this._trafficStops.Add(trafficStop);

        this.AssignUnitToTrafficStop(unit, trafficStop);
        
        return trafficStop;
    }
    
    public bool AssignUnitToTrafficStop(Unit unit, TrafficStop trafficStop)
    {
        if (!this._incidentService.AssignUnitToIncident(unit, trafficStop))
        {
            return false;
        }

        return true;
    }
}