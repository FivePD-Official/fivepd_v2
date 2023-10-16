using System;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Server.API;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.TrafficStopInterfaces;
using FivePD.Server.Models;
using Serilog;

namespace FivePD.Server.Services.TrafficStopServices;

#pragma warning disable

public class TrafficStopService : ITrafficStopService
{
    private readonly ILogger _logger;
    private readonly IInternalIncidentService _incidentService;

    public TrafficStopService(
        ILogger logger, 
        IIncidentService incidentService)
    {
        this._logger = logger;
        this._incidentService = (IInternalIncidentService)incidentService;
    }
    
    public TrafficStop GenerateTrafficStop(Unit unit, FVehicle vehicle)
    {
        if (unit is null || vehicle is null)
        {
            this._logger.Warning("TrafficStopService.GenerateTrafficStop: unit or vehicle is null");
            return null;
        }

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
        
        this._incidentService.InternalTrafficStops.Add(trafficStop);

        this.AssignUnitToTrafficStop(unit, trafficStop);
        
        return trafficStop;
    }
    
    public bool AssignUnitToTrafficStop(Unit unit, TrafficStop trafficStop)
    {
        return this._incidentService.AssignUnitToIncident(unit, trafficStop);
    }
}