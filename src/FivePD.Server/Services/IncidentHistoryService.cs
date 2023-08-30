using System.Linq;
using FivePD.Common.Enums;
using FivePD.Server.API;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

#pragma warning disable

internal class IncidentHistoryService : IIncidentHistoryService, IInternalIncidentHistoryService
{
    private readonly DatabaseContext _databaseContext;

    public IncidentHistoryService(DatabaseContext databaseContext)
    {
        this._databaseContext = databaseContext;
    }

    public void AddUnitToIncidentHistory(IIncident incident, FIncidentType incidentType, Unit unit)
    {
        var incidentHistory = this._databaseContext.IncidentHistories
            .FirstOrDefault(x => x.IncidentId == incident.Id.ToString());
        if (incidentHistory is null)
        {
            this.Create(new IncidentHistory
            {
                IncidentId = incident.Id.ToString(),
                Priority = incident.Priority,
                Type = incidentType,
                Title = incident.Title,
                Description = incident.Description,
                Location = incident.Location.ToString(),
                GeneratedAt = incident.GeneratedAt,
                Users = unit.Players.Select(x => new IncidentHistoryUser
                {
                    User = x.User,
                }).ToList(),
            });
            return;
        }

        foreach (var player in unit.Players)
        {
            this._databaseContext.IncidentHistoryUsers.Add(new IncidentHistoryUser
            {
                IncidentHistory = incidentHistory,
                User = player.User,
            });
        }

        this._databaseContext.SaveChangesAsync();
    }

    private void Create(IncidentHistory incidentHistory)
    {
        this._databaseContext.IncidentHistories.Add(incidentHistory);
        this._databaseContext.SaveChangesAsync();
    }
}