using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Server.API;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IIncidentHistoryService" />
[Obfuscate]
internal class IncidentHistoryService : IIncidentHistoryService, IInternalIncidentHistoryService
{
    private readonly DatabaseContext _databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentHistoryService"/> class.
    /// </summary>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    public IncidentHistoryService(DatabaseContext databaseContext)
    {
        this._databaseContext = databaseContext;
    }

    /// <inheritdoc/>
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
            incidentHistory.Users.Add(new IncidentHistoryUser
            {
                IncidentHistory = incidentHistory,
                User = player.User,
            });
        }

        this._databaseContext.SaveChanges();
    }

    private void Create(IncidentHistory incidentHistory)
    {
        this._databaseContext.IncidentHistories.Add(incidentHistory);
        this._databaseContext.SaveChanges();
    }

    [UIEvent(Events.IncidentHistory.GetAllPageable)]
    private void GetAllPageableUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IncidentHistoryGetAllRequest>();

        var filters = new Filters<IncidentHistory>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Id),
            incidentHistory => incidentHistory.Id.ToLower().Contains(request.Filters.Id.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Title),
            incidentHistory => incidentHistory.Title.ToLower().Contains(request.Filters.Title.ToLower()));
        filters.Add(
            request.Filters.Type != -1,
            incidentHistory => incidentHistory.Type == (FIncidentType)request.Filters.Type);

        var page = this._databaseContext.IncidentHistories
            .AsNoTracking()
            .Paginate(request.Page, filters);

        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.IncidentHistory.GetAllOfUser)]
    private void GetIncidentsOfUser(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var incidentHistories = this._databaseContext.IncidentHistories
            .Where(x => x.Users.Any(y => y.User.Id == request.Id))
            .Select(x => new SimpleIncidentHistoryDTO
            {
                Id = x.Id,
                Title = x.Title,
                GeneratedAt = x.GeneratedAt,
            });

        networkCallbackDelegate.InvokeWithJson(incidentHistories);
    }
}