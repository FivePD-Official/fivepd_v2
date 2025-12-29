using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.Models.Requests.Rank;
using FivePD.Server.Repositories;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class RankService : IRankService
{
    private readonly RankRepository _rankRepository;
    private readonly DepartmentRepository _departmentRepository;
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RankService"/> class.
    /// </summary>
    /// <param name="rankRepository">The <see cref="RankRepository"/> to use.</param>
    /// <param name="departmentRepository">The <see cref="DepartmentRepository"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public RankService(RankRepository rankRepository, DepartmentRepository departmentRepository, INetworkEventService networkEventService)
    {
        this._rankRepository = rankRepository;
        this._departmentRepository = departmentRepository;
        this._networkEventService = networkEventService;
    }

    /// <inheritdoc />
    public event EventHandler<Rank> RankCreated;

    /// <inheritdoc />
    public event EventHandler<Rank> RankUpdated;

    /// <inheritdoc />
    public event EventHandler<Rank> RankDeleted;

    [UIEvent(Events.Rank.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var rank = this._rankRepository.FindById(request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, rank);
    }

    [UIEvent(Events.Rank.GetAllGlobal)]
    private void GetAllGlobalUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var ranks = this._rankRepository.GetAllGlobal();
        this._networkEventService.TriggerEventSerialized(player, guid, ranks);
    }

    [UIEvent(Events.Rank.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<GetRanksPageRequest>(serializedRequest);
        var sqlBuilder = new SqlBuilder()
            .If(true, b => b.Where("Global = @Global", new { Global = request.Filters.Global }))
            .If(
                !request.Filters.Global,
                b => b.Where(
                    $"Id IN (SELECT RankId FROM {TableNames.DepartmentRanks} WHERE DepartmentId = @DepartmentId)",
                    new { DepartmentId = request.Filters.DepartmentId }))
            .If(!string.IsNullOrEmpty(request.Filters.Name), b => b.WhereContains("Name", request.Filters.Name));
        var page = this._rankRepository.Paginate(request.Page, sqlBuilder);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.Rank.Create)]
    private void CreateUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CreateRankRequest>(serializedRequest);
        var department = this._departmentRepository.FindById(request.DepartmentId);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var ranks = this._rankRepository.GetRanksByDepartment(request.Name, request.DepartmentId);
        if (ranks.Any())
        {
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "ERROR.RANK.CREATE.TITLE", "ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var rank = this._rankRepository.Add(new Rank
        {
            Global = false,
            Name = request.Name,
            Permission = request.Permission,
            RequiredXp = request.RequiredXp,
        });
        this._rankRepository.AttachDepartment(rank, department);
        this.CreateRankUI(player, guid, rank);
    }

    [UIEvent(Events.Rank.CreateGlobal)]
    private void CreateGlobalUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CreateGlobalRankRequest>(serializedRequest);
        var ranks = this._rankRepository.GetAllWhere("Name = @Name AND Global = @Global", new { Name = request.Name, Global = true });
        if (ranks.Any())
        {
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "ERROR.RANK.CREATE.TITLE", "ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var rank = this._rankRepository.Add(new Rank
        {
            Global = true,
            Name = request.Name,
            Permission = request.Permission,
            RequiredXp = request.RequiredXp,
        });
        this.CreateRankUI(player, guid, rank);
    }

    private void CreateRankUI(Player player, string guid, Rank rank)
    {
        this.RankCreated?.Invoke(this, rank);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.CREATE",
            "LABEL.RANK.SUCCESS.CREATE",
            new Dictionary<string, string>
            {
                { "rank", rank.Name },
            });
    }

    [UIEvent(Events.Rank.Edit)]
    private void EditUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditRankRequest>(serializedRequest);
        var rank = this._rankRepository.FindById(request.RankId);
        if (rank is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var ranksWithSameName = this._rankRepository.GetRanksByDepartment(request.Name, request.DepartmentId, request.RankId);
        if (ranksWithSameName.Any())
        {
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "ERROR.RANK.EDIT.TITLE", "ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        if (!this._departmentRepository.Exists(request.DepartmentId))
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var rankToEditOldName = rank.Name;
        this._rankRepository.Update(rank, x =>
        {
            x.Name = request.Name;
            x.Permission = request.Permission;
            x.RequiredXp = request.RequiredXp;
        });
        this.EditRankUI(player, guid, rank, rankToEditOldName);
    }

    [UIEvent(Events.Rank.EditGlobal)]
    private void EditGlobalUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditGlobalRankRequest>(serializedRequest);
        var rank = this._rankRepository.FindById(request.RankId);
        if (rank is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var ranksExistsWithName = this._rankRepository.GetAllWhere(
            "Name = @Name AND Global = @Global AND Id != @Id",
            new { Name = request.Name, Global = true, Id = request.RankId });
        if (ranksExistsWithName.Any())
        {
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "ERROR.RANK.EDIT.TITLE", "ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var rankToEditOldName = rank.Name;
        this._rankRepository.Update(rank, x =>
        {
            x.Name = request.Name;
            x.Permission = request.Permission;
            x.RequiredXp = request.RequiredXp;
        });
        this.EditRankUI(player, guid, rank, rankToEditOldName);
    }

    private void EditRankUI(Player player, string guid, Rank rank, string oldName)
    {
        this.RankUpdated?.Invoke(this, rank);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.EDIT",
            "LABEL.RANK.SUCCESS.EDIT",
            new Dictionary<string, string>
            {
                { "rank", oldName },
            });
    }

    [UIEvent(Events.Rank.Delete)]
    private void DeleteUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var rank = this._rankRepository.FindById(request.Id);
        if (rank is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this.RankDeleted?.Invoke(this, rank);
        this._rankRepository.Delete(rank.Id);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.DELETE",
            "LABEL.RANK.SUCCESS.DELETE",
            new Dictionary<string, string>
            {
                { "rank", rank.Name },
            });
    }
}