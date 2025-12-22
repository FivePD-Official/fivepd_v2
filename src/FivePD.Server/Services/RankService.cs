using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.Models.Requests.Rank;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class RankService : IRankService
{
    private readonly IDatabaseContextFactory _dbContextFactory;
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RankService"/> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="IDatabaseContextFactory"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public RankService(IDatabaseContextFactory dbContextFactory, INetworkEventService networkEventService)
    {
        this._dbContextFactory = dbContextFactory;
        this._networkEventService = networkEventService;
    }

    /// <inheritdoc />
    public event EventHandler<Rank> RankCreated;

    /// <inheritdoc />
    public event EventHandler<Rank> RankUpdated;

    /// <inheritdoc />
    public event EventHandler<Rank> RankDeleted;

    /// <inheritdoc />
    public Rank CreateRank(Rank rank)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var entityEntry = databaseContext.Ranks.Add(rank);
        databaseContext.SaveChanges();
        this.RankCreated?.Invoke(this, rank);
        return entityEntry.Entity;
    }

    /// <inheritdoc />
    public Rank UpdateRank(Rank rank)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var rankEntity = databaseContext.Ranks.FirstOrDefault(x => x.Id == rank.Id);
        if (rankEntity is null)
        {
            return null;
        }

        rankEntity.Name = rank.Name;
        rankEntity.DepartmentRanks = rank.DepartmentRanks;
        rankEntity.Global = rank.Global;
        rankEntity.RequiredXp = rank.RequiredXp;
        rankEntity.Permission = rank.Permission;
        rankEntity.Users = rank.Users;

        databaseContext.SaveChanges();
        this.RankUpdated?.Invoke(this, rank);
        return rankEntity;
    }

    /// <inheritdoc />
    public void DeleteRank(Rank rank)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var rankEntity = databaseContext.Ranks.FirstOrDefault(x => x.Id == rank.Id);
        if (rankEntity is null)
        {
            return;
        }

        databaseContext.Ranks.Remove(rankEntity);
        databaseContext.SaveChanges();
        this.RankDeleted?.Invoke(this, rank);
    }

    [UIEvent(Events.Rank.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var rank = databaseContext.Ranks
            .AsNoTracking()
            .FirstOrDefault(rank => rank.Id == request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, rank);
    }

    [UIEvent(Events.Rank.GetAllGlobal)]
    private void GetAllGlobalUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var rank = databaseContext.Ranks
            .AsNoTracking()
            .Where(rank => rank.Global);
        this._networkEventService.TriggerEventSerialized(player, guid, rank);
    }

    [UIEvent(Events.Rank.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<GetRanksPageRequest>(serializedRequest);

        var filters = new Filters<Rank>();
        filters.Add(true, rank => rank.Global == request.Filters.Global);
        filters.Add(!request.Filters.Global, rank => rank.DepartmentRanks.Any(x => x.DepartmentId == request.Filters.DepartmentId));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Name),
            department => department.Name.ToLower().Contains(request.Filters.Name.ToLower()));

        var page = databaseContext.Ranks.AsNoTracking().Include(x => x.DepartmentRanks).Paginate(request.Page, filters);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.Rank.Create)]
    private void CreateUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<CreateRankRequest>(serializedRequest);
        var rankWithSameName = databaseContext.Ranks
            .Include(rank => rank.DepartmentRanks)
            .FirstOrDefault(rank => rank.Name == request.Name && !rank.Global);

        // Return if there is a rank with the same for the given department
        if (rankWithSameName?.DepartmentRanks.FirstOrDefault(x => x.DepartmentId == request.DepartmentId) is not null)
        {
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "ERROR.RANK.CREATE.TITLE", "ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var department = databaseContext.Departments.Include(x => x.DepartmentRanks).FirstOrDefault(x => x.Id == request.DepartmentId);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var createdRankEntry = databaseContext.Ranks.Add(new Rank
        {
            Global = false,
            Name = request.Name,
            Permission = request.Permission,
            RequiredXp = request.RequiredXp,
        });
        var departmentRank = new DepartmentRank { Rank = createdRankEntry.Entity, Department = department };
        department.DepartmentRanks.Add(departmentRank);
        this.CreateRankUI(player, databaseContext, guid, createdRankEntry.Entity);
    }

    [UIEvent(Events.Rank.CreateGlobal)]
    private void CreateGlobalUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<CreateGlobalRankRequest>(serializedRequest);
        var rankExistsWithName = databaseContext.Ranks.FirstOrDefault(rank => rank.Name == request.Name && rank.Global);
        if (rankExistsWithName is not null)
        {
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "ERROR.RANK.CREATE.TITLE", "ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var createdRankEntry = databaseContext.Ranks.Add(new Rank
        {
            Global = true,
            Name = request.Name,
            Permission = request.Permission,
            RequiredXp = request.RequiredXp,
        });
        this.CreateRankUI(player, databaseContext, guid, createdRankEntry.Entity);
    }

    private void CreateRankUI(Player player, DatabaseContext databaseContext, string guid, Rank rank)
    {
        databaseContext.SaveChanges();
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
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<EditRankRequest>(serializedRequest);
        var rankToEdit = databaseContext.Ranks.FirstOrDefault(rank => rank.Id == request.RankId);
        if (rankToEdit is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var rankWithSameName = databaseContext.Ranks
            .Include(rank => rank.DepartmentRanks)
            .FirstOrDefault(rank => rank.Name == request.Name && !rank.Global && rank.Id != rankToEdit.Id);

        // Return if there is a rank with the same for the given department
        if (rankWithSameName?.DepartmentRanks.FirstOrDefault(x => x.DepartmentId == request.DepartmentId) is not null)
        {
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "ERROR.RANK.EDIT.TITLE", "ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var department = databaseContext.Departments.Include(x => x.DepartmentRanks).FirstOrDefault(x => x.Id == request.DepartmentId);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var rankToEditOldName = rankToEdit.Name;
        rankToEdit.Name = request.Name;
        rankToEdit.Permission = request.Permission;
        rankToEdit.RequiredXp = request.RequiredXp;
        this.EditRankUI(player, databaseContext, guid, rankToEdit, rankToEditOldName);
    }

    [UIEvent(Events.Rank.EditGlobal)]
    private void EditGlobalUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<EditGlobalRankRequest>(serializedRequest);
        var rankToEdit = databaseContext.Ranks.FirstOrDefault(rank => rank.Id == request.RankId);
        if (rankToEdit is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var rankExistsWithName = databaseContext.Ranks.FirstOrDefault(rank => rank.Name == request.Name && rank.Global && rank.Id != rankToEdit.Id);
        if (rankExistsWithName is not null)
        {
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "ERROR.RANK.EDIT.TITLE", "ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var rankToEditOldName = rankToEdit.Name;
        rankToEdit.Name = request.Name;
        rankToEdit.Permission = request.Permission;
        rankToEdit.RequiredXp = request.RequiredXp;
        this.EditRankUI(player, databaseContext, guid, rankToEdit, rankToEditOldName);
    }

    private void EditRankUI(Player player, DatabaseContext databaseContext, string guid, Rank rank, string oldName)
    {
        databaseContext.SaveChanges();
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
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var rankToDelete = databaseContext.Ranks
            .Include(rank => rank.Users)
            .FirstOrDefault(rank => rank.Id == request.Id);
        if (rankToDelete is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var rankToDeleteName = rankToDelete.Name;
        foreach (var user in rankToDelete.Users)
        {
            user.RankId = null;
        }

        databaseContext.Ranks.Remove(rankToDelete);
        databaseContext.SaveChanges();
        this.RankDeleted?.Invoke(this, rankToDelete);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.DELETE",
            "LABEL.RANK.SUCCESS.DELETE",
            new Dictionary<string, string>
            {
                { "rank", rankToDeleteName },
            });
    }
}