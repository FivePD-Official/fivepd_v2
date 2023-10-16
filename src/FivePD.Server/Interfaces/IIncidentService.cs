using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FivePD.Server.API;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IIncidentService
{
    public event EventHandler<Callout> OnCalloutCompleted;

    public void CompleteIncidentOfPlayer(FPlayer player);

    public void CompleteIncident(IIncident incident);

    public ReadOnlyCollection<Callout> ActiveCallouts { get; }

    public ReadOnlyCollection<TrafficStop> TrafficStops { get; }
}

internal interface IInternalIncidentService : IIncidentService
{
    public bool AssignUnitToIncident(Unit unit, IIncident incident);
    
    public List<InternalCallout> InternalCallouts { get; }

    public List<TrafficStop> InternalTrafficStops { get; }
}