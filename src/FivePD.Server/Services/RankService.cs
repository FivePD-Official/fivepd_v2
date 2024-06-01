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

namespace FivePD.Server.Services;

/// <inheritdoc />
public class RankService : IRankService
{
    private readonly DatabaseContext _databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RankService"/> class.
    /// </summary>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    public RankService(DatabaseContext databaseContext)
    {
        this._databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public event EventHandler<Rank> RankCreated;

    /// <inheritdoc />
    public event EventHandler<Rank> RankUpdated;

    /// <inheritdoc />
    public event EventHandler<Rank> RankDeleted;

    [UIEvent(Events.Rank.Get)]
    private void GetUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var rank = this._databaseContext.Ranks
            .AsNoTracking()
            .FirstOrDefault(rank => rank.Id == request.Id);
        networkCallbackDelegate.InvokeWithJson(rank);
    }

    [UIEvent(Events.Rank.GetAllGlobal)]
    private void GetAllGlobalUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var rank = this._databaseContext.Ranks
            .AsNoTracking()
            .Where(rank => rank.Global);
        networkCallbackDelegate.InvokeWithJson(rank);
    }

    [UIEvent(Events.Rank.GetAllPageable)]
    private void GetAllPageableUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<GetRanksPageRequest>();

        var filters = new Filters<Rank>();
        filters.Add(true, rank => rank.Global == request.Filters.Global);
        filters.Add(!request.Filters.Global, rank => rank.DepartmentRanks.Any(x => x.DepartmentId == request.Filters.DepartmentId));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Name),
            department => department.Name.ToLower().Contains(request.Filters.Name.ToLower()));

        var page = this._databaseContext.Ranks.AsNoTracking().Include(x => x.DepartmentRanks).Paginate(request.Page, filters);
        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.Rank.Create)]
    private void CreateUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<CreateRankRequest>();
        var rankWithSameName = this._databaseContext.Ranks
            .Include(rank => rank.DepartmentRanks)
            .FirstOrDefault(rank => rank.Name == request.Name && !rank.Global);

        // Return if there is a rank with the same for the given department
        if (rankWithSameName?.DepartmentRanks.FirstOrDefault(x => x.DepartmentId == request.DepartmentId) is not null)
        {
            networkCallbackDelegate.InvokeWithResponse(false, "UI.ERROR.RANK.CREATE.TITLE", "UI.ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var department = this._databaseContext.Departments.Include(x => x.DepartmentRanks).FirstOrDefault(x => x.Id == request.DepartmentId);
        if (department is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var createdRankEntry = this._databaseContext.Ranks.Add(new Rank
        {
            Global = false,
            Name = request.Name,
            Permission = request.Permission,
            RequiredXp = request.RequiredXp,
        });
        var departmentRank = new DepartmentRank { Rank = createdRankEntry.Entity, Department = department };
        department.DepartmentRanks.Add(departmentRank);
        this.CreateRankUI(createdRankEntry.Entity, networkCallbackDelegate);
    }

    [UIEvent(Events.Rank.CreateGlobal)]
    private void CreateGlobalUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<CreateGlobalRankRequest>();
        var rankExistsWithName = this._databaseContext.Ranks.FirstOrDefault(rank => rank.Name == request.Name && rank.Global);
        if (rankExistsWithName is not null)
        {
            networkCallbackDelegate.InvokeWithResponse(false, "UI.ERROR.RANK.CREATE.TITLE", "UI.ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var createdRankEntry = this._databaseContext.Ranks.Add(new Rank
        {
            Global = true,
            Name = request.Name,
            Permission = request.Permission,
            RequiredXp = request.RequiredXp,
        });
        this.CreateRankUI(createdRankEntry.Entity, networkCallbackDelegate);
    }

    private void CreateRankUI(Rank rank, NetworkCallbackDelegate networkCallbackDelegate)
    {
        this._databaseContext.SaveChanges();
        this.RankCreated?.Invoke(this, rank);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.CREATE",
            "UI.LABEL.RANK.SUCCESS.CREATE",
            new Dictionary<string, string>
            {
                { "rank", rank.Name },
            });
    }

    [UIEvent(Events.Rank.Edit)]
    private void EditUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<EditRankRequest>();
        var rankToEdit = this._databaseContext.Ranks.FirstOrDefault(rank => rank.Id == request.RankId);
        if (rankToEdit is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var rankWithSameName = this._databaseContext.Ranks
            .Include(rank => rank.DepartmentRanks)
            .FirstOrDefault(rank => rank.Name == request.Name && !rank.Global && rank.Id != rankToEdit.Id);

        // Return if there is a rank with the same for the given department
        if (rankWithSameName?.DepartmentRanks.FirstOrDefault(x => x.DepartmentId == request.DepartmentId) is not null)
        {
            networkCallbackDelegate.InvokeWithResponse(false, "UI.ERROR.RANK.EDIT.TITLE", "UI.ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var department = this._databaseContext.Departments.Include(x => x.DepartmentRanks).FirstOrDefault(x => x.Id == request.DepartmentId);
        if (department is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var rankToEditOldName = rankToEdit.Name;
        rankToEdit.Name = request.Name;
        rankToEdit.Permission = request.Permission;
        rankToEdit.RequiredXp = request.RequiredXp;
        this.EditRankUI(rankToEdit, rankToEditOldName, networkCallbackDelegate);
    }

    [UIEvent(Events.Rank.EditGlobal)]
    private void EditGlobalUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<EditGlobalRankRequest>();
        var rankToEdit = this._databaseContext.Ranks.FirstOrDefault(rank => rank.Id == request.RankId);
        if (rankToEdit is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var rankExistsWithName = this._databaseContext.Ranks.FirstOrDefault(rank => rank.Name == request.Name && rank.Global && rank.Id != rankToEdit.Id);
        if (rankExistsWithName is not null)
        {
            networkCallbackDelegate.InvokeWithResponse(false, "UI.ERROR.RANK.EDIT.TITLE", "UI.ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var rankToEditOldName = rankToEdit.Name;
        rankToEdit.Name = request.Name;
        rankToEdit.Permission = request.Permission;
        rankToEdit.RequiredXp = request.RequiredXp;
        this.EditRankUI(rankToEdit, rankToEditOldName, networkCallbackDelegate);
    }

    private void EditRankUI(Rank rank, string oldName, NetworkCallbackDelegate networkCallbackDelegate)
    {
        this._databaseContext.SaveChanges();
        this.RankUpdated?.Invoke(this, rank);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.EDIT",
            "UI.LABEL.RANK.SUCCESS.EDIT",
            new Dictionary<string, string>
            {
                { "rank", oldName },
            });
    }

    [UIEvent(Events.Rank.Delete)]
    private void DeleteUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var rankToDelete = this._databaseContext.Ranks
            .Include(rank => rank.Users)
            .FirstOrDefault(rank => rank.Id == request.Id);
        if (rankToDelete is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var rankToDeleteName = rankToDelete.Name;
        foreach (var user in rankToDelete.Users)
        {
            user.RankId = null;
        }

        this._databaseContext.Ranks.Remove(rankToDelete);
        this._databaseContext.SaveChanges();
        this.RankDeleted?.Invoke(this, rankToDelete);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.DELETE",
            "UI.LABEL.RANK.SUCCESS.DELETE",
            new Dictionary<string, string>
            {
                { "rank", rankToDeleteName },
            });
    }
}