using FivePD.Common.Enums;
using FivePD.Server.API;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IIncidentHistoryService
{
}

internal interface IInternalIncidentHistoryService : IIncidentHistoryService
{
    public void AddUnitToIncidentHistory(IIncident incident, FIncidentType incidentType, Unit unit);
}