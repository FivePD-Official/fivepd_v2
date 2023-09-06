using FivePD.Common.Enums;
using FivePD.Server.API;
using FivePD.Server.Models;
using FivePD.Server.Services;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Represents a service that manages the incident histories.
/// </summary>
public interface IIncidentHistoryService
{
}

/// <summary>
/// Internal interface for the <see cref="IncidentHistoryService"/> class.
/// </summary>
internal interface IInternalIncidentHistoryService
{
    /// <summary>
    /// Creates a new incident history.
    /// </summary>
    /// <param name="incident">The incident.</param>
    /// <param name="incidentType">The type of the incident.</param>
    /// <param name="unit">The unit to add.</param>
    public void AddUnitToIncidentHistory(IIncident incident, FIncidentType incidentType, Unit unit);
}