using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Server.API;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Requests;
using FivePD.Server.Repositories;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IIncidentHistoryService" />
[Obfuscate]
public class IncidentHistoryService : IIncidentHistoryService, IInternalIncidentHistoryService
{
    private readonly IncidentHistoryRepository _incidentHistoryRepository;
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentHistoryService"/> class.
    /// </summary>
    /// <param name="incidentHistoryRepository">The <see cref="IncidentHistoryRepository"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public IncidentHistoryService(
        IncidentHistoryRepository incidentHistoryRepository,
        INetworkEventService networkEventService)
    {
        this._incidentHistoryRepository = incidentHistoryRepository;
        this._networkEventService = networkEventService;
    }

    /// <inheritdoc/>
    public void AddUnitToIncidentHistory(IIncident incident, FIncidentType incidentType, Unit unit)
    {
        var incidentHistory = this._incidentHistoryRepository.FindById(incident.Id.ToString());
        if (incidentHistory is null)
        {
            this._incidentHistoryRepository.Add(new IncidentHistory
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
            this._incidentHistoryRepository.AttachUser(incidentHistory, player.User);
        }
    }

    [UIEvent(Events.IncidentHistory.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IncidentHistoryGetAllRequest>(serializedRequest);
        var page = this._incidentHistoryRepository.Paginate(
            request.Page,
            b => b
                .If(!string.IsNullOrEmpty(request.Filters.Id), x => x.WhereContains("Id", request.Filters.Id))
                .If(!string.IsNullOrEmpty(request.Filters.Title), x => x.WhereContains("Title", request.Filters.Title))
                .If(request.Filters.Type != -1, x => x.WhereEquals("Type", request.Filters.Type)));
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.IncidentHistory.GetAllOfUser)]
    private void GetIncidentsOfUser([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var incidentHistories = this._incidentHistoryRepository
            .GetAllOfUser(request.Id)
            .Select(x => new SimpleIncidentHistoryDTO
            {
                Id = x.Id,
                Title = x.Title,
                GeneratedAt = x.GeneratedAt,
            });
        this._networkEventService.TriggerEventSerialized(player, guid, incidentHistories);
    }
}