using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Server.API;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IIncidentHistoryService" />
[Obfuscate]
public class IncidentHistoryService : IIncidentHistoryService, IInternalIncidentHistoryService
{
    private readonly IDatabaseContextFactory _dbContextFactory;
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentHistoryService"/> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="IDatabaseContextFactory"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public IncidentHistoryService(IDatabaseContextFactory dbContextFactory, INetworkEventService networkEventService)
    {
        this._dbContextFactory = dbContextFactory;
        this._networkEventService = networkEventService;
    }

    /// <inheritdoc/>
    public void AddUnitToIncidentHistory(IIncident incident, FIncidentType incidentType, Unit unit)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var incidentHistory = databaseContext.IncidentHistories
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

        databaseContext.SaveChanges();
    }

    private void Create(IncidentHistory incidentHistory)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        databaseContext.IncidentHistories.Add(incidentHistory);
        databaseContext.SaveChanges();
    }

    [UIEvent(Events.IncidentHistory.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<IncidentHistoryGetAllRequest>(serializedRequest);

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

        var page = databaseContext.IncidentHistories
            .AsNoTracking()
            .Paginate(request.Page, filters);

        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.IncidentHistory.GetAllOfUser)]
    private void GetIncidentsOfUser([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var incidentHistories = databaseContext.IncidentHistories
            .Where(x => x.Users.Any(y => y.User.Id == request.Id))
            .Select(x => new SimpleIncidentHistoryDTO
            {
                Id = x.Id,
                Title = x.Title,
                GeneratedAt = x.GeneratedAt,
            });

        this._networkEventService.TriggerEventSerialized(player, guid, incidentHistories);
    }
}