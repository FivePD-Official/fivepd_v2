using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Extensions;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Exceptions;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Events;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using FivePD.Server.Utilities;
using Microsoft.EntityFrameworkCore;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class DepartmentService : IDepartmentService
{
    private readonly DatabaseContext _databaseContext;
    private readonly IPlayerDataService _playerDataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DepartmentService"/> class.
    /// </summary>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    /// <param name="playerDataService">The <see cref="PlayerDataService"/> to use.</param>
    public DepartmentService(DatabaseContext databaseContext, IPlayerDataService playerDataService)
    {
        this._databaseContext = databaseContext;
        this._playerDataService = playerDataService;
    }

    /// <inheritdoc />
    public event EventHandler<Department> DepartmentCreated;

    /// <inheritdoc />
    public event EventHandler<Department> DepartmentUpdated;

    /// <inheritdoc />
    public event EventHandler<Department> DepartmentDeleted;

    /// <inheritdoc />
    public event EventHandler<PlayerDepartmentChangeEventArgs> PlayerJoinedDepartment;

    /// <inheritdoc />
    public event EventHandler<PlayerDepartmentChangeEventArgs> PlayerLeftDepartment;

    /// <inheritdoc />
    public event EventHandler<Rank> RankCreated;

    /// <inheritdoc />
    public event EventHandler<Rank> RankUpdated;

    /// <inheritdoc />
    public event EventHandler<Rank> RankDeleted;

    /// <inheritdoc />
    public event EventHandler<Division> DivisionCreated;

    /// <inheritdoc />
    public event EventHandler<Division> DivisionUpdated;

    /// <inheritdoc />
    public event EventHandler<Division> DivisionDeleted;

    /// <inheritdoc />
    public Department CreateDepartment(Department department)
    {
        if (department is null)
        {
            throw new ArgumentException("The provided department is null");
        }

        if (string.IsNullOrEmpty(department.Name))
        {
            throw new ArgumentException("The name of the department can't be null or empty");
        }

        var departmentExists = this._databaseContext.Departments.FirstOrDefault(d => d.Name == department.Name);
        if (departmentExists is not null)
        {
            throw new DepartmentExistsWithNameException();
        }

        if (string.IsNullOrEmpty(department.Callsign))
        {
            department.Callsign = department.Name.ToAcronym();
        }

        var createdDepartmentEntry = this._databaseContext.Departments.Add(department);
        this._databaseContext.SaveChanges();
        this.DepartmentCreated?.Invoke(this, createdDepartmentEntry.Entity);
        return createdDepartmentEntry.Entity;
    }

    /// <inheritdoc />
    public void RemoveDepartment(Department department)
    {
        this._databaseContext.Departments.Remove(department);
        this._databaseContext.SaveChanges();
    }

    /// <inheritdoc />
    public IEnumerable<Department> GetDepartments(DepartmentQueryOptions options = default)
    {
        return this._databaseContext.Departments
            .IncludeIf(options.IncludeDivisions, dept => dept.Divisions)
            .IncludeIf(options.IncludeRanks, dept => dept.Ranks)
            .IncludeIf(options.IncludeUsers, dept => dept.Users);
    }

    /// <inheritdoc />
    public Department GetDepartmentById(int id, DepartmentQueryOptions options = default)
    {
        return this._databaseContext.Departments
            .IncludeIf(options.IncludeDivisions, dept => dept.Divisions)
            .IncludeIf(options.IncludeRanks, dept => dept.Ranks)
            .IncludeIf(options.IncludeUsers, dept => dept.Users)
            .FirstOrDefault(department => department.Id == id);
    }

    /// <inheritdoc />
    public void AddPlayerToDepartment(User user, Department department)
    {
        if (user is null || department is null)
        {
            throw new ArgumentException("Some of the provided arguments is null");
        }

        if (department.MaxUsers != 0 && department.MaxUsers == department.Users.Count())
        {
            throw new DepartmentFullException();
        }

        user.DepartmentId = department.Id;
        user.DivisionId = null;
        user.RankId = null;
        this._databaseContext.SaveChanges();
        this.PlayerJoinedDepartment?.Invoke(this, new PlayerDepartmentChangeEventArgs(user, department));
    }

    /// <inheritdoc />
    public void AddPlayerToDepartment(Player player, Department department)
    {
        this.AddPlayerToDepartment(this._playerDataService.FindUserByLicense(player), department);
    }

    /// <inheritdoc />
    public void RemovePlayerFromDepartment(User user)
    {
        if (user is null)
        {
            throw new ArgumentException("The provided user is null");
        }

        var department = user.Department;
        user.DepartmentId = null;
        user.DivisionId = null;
        user.RankId = null;
        user.Callsign = string.Empty;
        this._databaseContext.SaveChanges();
        this.PlayerLeftDepartment?.Invoke(this, new PlayerDepartmentChangeEventArgs(user, department));
    }

    /// <inheritdoc />
    public void RemovePlayerFromDepartment(Player player)
    {
        this.RemovePlayerFromDepartment(this._playerDataService.FindUserByLicense(player));
    }

    /// <inheritdoc />
    public Department GetPlayerDepartment(FPlayer player)
    {
        var user = player.User;
        if (user is null)
        {
            return null;
        }

        return this._databaseContext.Departments
            .Include(x => x.Users)
            .FirstOrDefault(department => department.Users.Any(x => x.Id == user.Id));
    }

    [UIEvent(Events.UI.Department.GetAll)]
    private void GetAllUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var departments = this._databaseContext.Departments
            .AsNoTracking()
            .Select(x => new SimpleDepartmentDTO
            {
                Id = x.Id,
                Name = x.Name,
            });
        networkCallbackDelegate.InvokeWithJson(departments);
    }

    [UIEvent(Events.UI.Department.GetAllPageable)]
    private void GetAllPageableUI(ExpandoObject departmentsRequestString, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var departmentsRequest = departmentsRequestString.ConvertTo<DepartmentsRequest>();
        var filters = new Filters<Department>();
        filters.Add(
            !string.IsNullOrEmpty(departmentsRequest.Filters.Name),
            department => department.Name.ToLower().Contains(departmentsRequest.Filters.Name.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(departmentsRequest.Filters.Callsign),
            department => department.Callsign.ToLower().Contains(departmentsRequest.Filters.Callsign.ToLower()));
        var page = this._databaseContext.Departments
            .AsNoTracking()
            .Include(department => department.Users)
            .Paginate(departmentsRequest.Page, filters);

        page.Items = page.Items.Select(item => new DepartmentResponse(item)).ToList();

        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.Department.Get)]
    private void GetUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var department = this._databaseContext.Departments
            .AsNoTracking()
            .Include(dept => dept.Divisions)
            .Include(dept => dept.Ranks)
            .FirstOrDefault(dept => dept.Id == request.Id);
        networkCallbackDelegate.InvokeWithJson(department);
    }

    [UIEvent(Events.UI.Department.GetUsers)]
    private void GetDepartmentUsersForNui(ExpandoObject departmentUsersRequestString, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var departmentUsersRequest = departmentUsersRequestString.ConvertTo<DepartmentUsersRequest>();

        var filters = new Filters<User>();
        filters.Add(true, user => user.Department.Id == departmentUsersRequest.DepartmentId);
        filters.Add(
            !string.IsNullOrEmpty(departmentUsersRequest.Filters.Name),
            department => department.Name.ToLower().Contains(departmentUsersRequest.Filters.Name.ToLower()));

        var page = this._databaseContext.Users.AsNoTracking().Paginate(departmentUsersRequest.Page, filters);
        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.Department.GetRank)]
    private void GetDepartmentRankForNui(ExpandoObject requestString, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = requestString.ConvertTo<IdRequest>();
        var rank = this._databaseContext.Ranks
            .AsNoTracking()
            .FirstOrDefault(rank => rank.Id == request.Id);
        networkCallbackDelegate.InvokeWithJson(rank);
    }

    [UIEvent(Events.UI.Department.GetRanks)]
    private void GetDepartmentRanksForNui(ExpandoObject departmentRanksRequestString, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var departmentRanksRequest = departmentRanksRequestString.ConvertTo<DepartmentRanksRequest>();

        var filters = new Filters<Rank>();
        filters.Add(true, rank => rank.Department.Id == departmentRanksRequest.DepartmentId);
        filters.Add(
            !string.IsNullOrEmpty(departmentRanksRequest.Filters.Name),
            department => department.Name.ToLower().Contains(departmentRanksRequest.Filters.Name.ToLower()));

        var page = this._databaseContext.Ranks.AsNoTracking().Paginate(departmentRanksRequest.Page, filters);
        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.Department.GetDivision)]
    private void GetDepartmentDivisionForNui(ExpandoObject requestString, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = requestString.ConvertTo<IdRequest>();

        var division = this._databaseContext.Divisions
            .AsNoTracking()
            .FirstOrDefault(division => division.Id == request.Id);
        networkCallbackDelegate.InvokeWithJson(division);
    }

    [UIEvent(Events.UI.Department.GetDivisions)]
    private void GetDepartmentDivisionsForNui(ExpandoObject departmentDivisionsRequestString, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var departmentDivisionsRequest =
            departmentDivisionsRequestString.ConvertTo<DepartmentDivisionsRequest>();

        var filters = new Filters<Division>();
        filters.Add(true, division => division.Department.Id == departmentDivisionsRequest.DepartmentId);
        filters.Add(
            !string.IsNullOrEmpty(departmentDivisionsRequest.Filters.Name),
            department => department.Name.ToLower().Contains(departmentDivisionsRequest.Filters.Name.ToLower()));

        var page = this._databaseContext.Divisions
            .AsNoTracking()
            .Paginate(departmentDivisionsRequest.Page, filters);
        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.Department.Create)]
    private void CreateDepartmentFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var createDepartmentRequest = expandoObject.ConvertTo<CreateDepartmentRequest>();
            this.CreateDepartment(new Department
            {
                Name = createDepartmentRequest.Name,
                CallsignFormat = createDepartmentRequest.CallsignFormat,
                MaxUsers = createDepartmentRequest.MaxUsers,
            });
            networkCallbackDelegate.InvokeWithResponse(
                true,
                "UI.LABEL.SUCCESS.CREATE",
                "UI.LABEL.DEPARTMENT.SUCCESS.CREATE",
                new Dictionary<string, string>
                {
                    { "department", createDepartmentRequest.Name },
                });
        }
        catch (DepartmentExistsWithNameException)
        {
            networkCallbackDelegate.InvokeWithResponse(
                false,
                "UI.ERROR.DEPARTMENT.CREATE.TITLE",
                "UI.ERROR.DEPARTMENT.NAME_CONFLICT.DESCRIPTION");
        }
    }

    [UIEvent(Events.UI.Department.DeleteDivision)]
    private void DeleteDivisionFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var divisionToDelete = this._databaseContext.Divisions
            .Include(division => division.Users)
            .Include(division => division.Department)
            .FirstOrDefault(division => division.Id == request.Id);
        if (divisionToDelete is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var divisionToDeleteName = divisionToDelete.Name;
        foreach (var user in divisionToDelete.Users)
        {
            user.DivisionId = null;
        }

        this._databaseContext.Divisions.Remove(divisionToDelete);
        this._databaseContext.SaveChanges();
        this.DivisionDeleted?.Invoke(this, divisionToDelete);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.DELETE",
            "UI.LABEL.DIVISION.SUCCESS.DELETE",
            new Dictionary<string, string>
            {
                { "division", divisionToDeleteName },
            });
    }

    [UIEvent(Events.UI.Department.EditDivision)]
    private void EditDivisionFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var renameDivisionRequest = expandoObject.ConvertTo<EditDivisionRequest>();
        var divisionToEdit =
            this._databaseContext.Divisions.FirstOrDefault(division =>
                division.Id == renameDivisionRequest.DivisionId);
        if (divisionToEdit is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var divisionExistsWithName = this._databaseContext.Divisions
            .FirstOrDefault(division => division.Name == renameDivisionRequest.Name &&
                                        division.Department.Id == renameDivisionRequest.DepartmentId &&
                                        division.Id != divisionToEdit.Id);
        if (divisionExistsWithName != null)
        {
            networkCallbackDelegate.InvokeWithResponse(
                false,
                "UI.ERROR.DIVISION.EDIT.TITLE",
                "UI.ERROR.DIVISION.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var divisionToEditOldName = divisionToEdit.Name;
        divisionToEdit.Name = renameDivisionRequest.Name;
        this._databaseContext.SaveChanges();
        this.DivisionUpdated?.Invoke(this, divisionToEdit);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.EDIT",
            "UI.LABEL.DIVISION.SUCCESS.EDIT",
            new Dictionary<string, string>
            {
                { "division", divisionToEditOldName },
            });
    }

    [UIEvent(Events.UI.Department.CreateDivision)]
    private void CreateDivisionFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var createDivisionRequest = expandoObject.ConvertTo<CreateDivisionRequest>();
        var divisionExistsWithName = this._databaseContext.Divisions.FirstOrDefault(division =>
            division.Name == createDivisionRequest.Name && division.Department.Id == createDivisionRequest.DepartmentId);
        if (divisionExistsWithName != null)
        {
            networkCallbackDelegate.InvokeWithResponse(
                false,
                "UI.ERROR.DIVISION.CREATE.TITLE",
                "UI.ERROR.DIVISION.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var department = this._databaseContext.Departments.FirstOrDefault(department => department.Id == createDivisionRequest.DepartmentId);
        if (department is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var createdDivisionEntry = this._databaseContext.Divisions.Add(new Division
        {
            Department = department,
            Name = createDivisionRequest.Name,
        });
        this._databaseContext.SaveChanges();
        this.DivisionCreated?.Invoke(this, createdDivisionEntry.Entity);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.CREATE",
            "UI.LABEL.DIVISION.SUCCESS.CREATE",
            new Dictionary<string, string>
            {
                { "division", createDivisionRequest.Name },
            });
    }

    [UIEvent(Events.UI.Department.DeleteRank)]
    private void DeleteRankFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var rankToDelete = this._databaseContext.Ranks
            .Include(rank => rank.Users)
            .Include(rank => rank.Department)
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

        if (rankToDelete.Department.DefaultRankId == rankToDelete.Id)
        {
            rankToDelete.Department.DefaultRankId = null;
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

    [UIEvent(Events.UI.Department.EditRank)]
    private void EditRankFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var editRankRequest = expandoObject.ConvertTo<EditRankRequest>();
        var rankToEdit = this._databaseContext.Ranks.FirstOrDefault(rank => rank.Id == editRankRequest.RankId);
        if (rankToEdit is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var rankExistsWithName = this._databaseContext.Ranks.FirstOrDefault(rank =>
            rank.Name == editRankRequest.Name && rank.Department.Id == editRankRequest.DepartmentId && rank.Id != rankToEdit.Id);
        if (rankExistsWithName != null)
        {
            networkCallbackDelegate.InvokeWithResponse(false, "UI.ERROR.RANK.EDIT.TITLE", "UI.ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var rankToEditOldName = rankToEdit.Name;
        rankToEdit.Name = editRankRequest.Name;
        rankToEdit.Permission = editRankRequest.Permission;
        rankToEdit.RequiredXp = editRankRequest.RequiredXp;
        this._databaseContext.SaveChanges();
        this.RankUpdated?.Invoke(this, rankToEdit);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.EDIT",
            "UI.LABEL.RANK.SUCCESS.EDIT",
            new Dictionary<string, string>
            {
                { "rank", rankToEditOldName },
            });
    }

    [UIEvent(Events.UI.Department.CreateRank)]
    private void CreateRankFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var createRankRequest = expandoObject.ConvertTo<CreateRankRequest>();
        var rankExistsWithName = this._databaseContext.Ranks.FirstOrDefault(rank =>
            rank.Name == createRankRequest.Name && rank.Department.Id == createRankRequest.DepartmentId);
        if (rankExistsWithName != null)
        {
            networkCallbackDelegate.InvokeWithResponse(false, "UI.ERROR.RANK.CREATE.TITLE", "UI.ERROR.RANK.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var department = this._databaseContext.Departments.FirstOrDefault(department =>
            department.Id == createRankRequest.DepartmentId);
        if (department is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var createdRankEntry = this._databaseContext.Ranks.Add(new Rank
        {
            Department = department,
            Name = createRankRequest.Name,
            Permission = createRankRequest.Permission,
            RequiredXp = createRankRequest.RequiredXp,
        });
        this._databaseContext.SaveChanges();
        this.RankCreated?.Invoke(this, createdRankEntry.Entity);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.CREATE",
            "UI.LABEL.RANK.SUCCESS.CREATE",
            new Dictionary<string, string>
            {
                { "rank", createRankRequest.Name },
            });
    }

    [UIEvent(Events.UI.Department.Edit)]
    private void EditDepartmentFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var editDepartmentRequest = expandoObject.ConvertTo<EditDepartmentRequest>();
        var department = this._databaseContext.Departments.FirstOrDefault(department =>
            department.Id == editDepartmentRequest.DepartmentId);
        if (department is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        var departmentExistsWithName = this._databaseContext.Departments.AsNoTracking().FirstOrDefault(dept =>
            dept.Id != editDepartmentRequest.DepartmentId && dept.Name == editDepartmentRequest.Name);
        if (departmentExistsWithName != null)
        {
            networkCallbackDelegate.InvokeWithResponse(
                false,
                "UI.ERROR.DEPARTMENT.EDIT.TITLE",
                "UI.ERROR.DEPARTMENT.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        if (editDepartmentRequest.DefaultRankId != -1)
        {
            var defaultRank = this._databaseContext.Ranks.FirstOrDefault(rank => rank.Id == editDepartmentRequest.DefaultRankId);
            if (defaultRank is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }
        }

        var departmentOldName = department.Name;
        department.Name = editDepartmentRequest.Name;
        department.Callsign = string.IsNullOrEmpty(editDepartmentRequest.Callsign)
            ? editDepartmentRequest.Name.ToAcronym()
            : editDepartmentRequest.Callsign;
        department.CallsignFormat = editDepartmentRequest.CallsignFormat;
        department.MaxUsers = editDepartmentRequest.MaxUsers;
        department.DefaultRankId = editDepartmentRequest.DefaultRankId == -1 ? (int?)null : editDepartmentRequest.DefaultRankId;
        this._databaseContext.SaveChanges();
        this.DepartmentUpdated?.Invoke(this, department);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.EDIT",
            "UI.LABEL.DEPARTMENT.SUCCESS.EDIT",
            new Dictionary<string, string>
            {
                { "department", departmentOldName },
            });
    }

    [UIEvent(Events.UI.Department.CheckIfDepartmentNameTaken)]
    private void CheckIfDepartmentNameTaken(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<NameTakenRequest>();
        Department departmentExists;
        if (request.CheckForDepartmentId)
        {
            departmentExists =
                this._databaseContext.Departments.FirstOrDefault(department =>
                    department.Name == request.Name && department.Id != request.DepartmentId);
        }
        else
        {
            departmentExists = this._databaseContext.Departments.FirstOrDefault(department => department.Name == request.Name);
        }

        networkCallbackDelegate.InvokeWithJson(new NameTakenResponse(departmentExists != null));
    }

    [UIEvent(Events.UI.Department.Delete)]
    private void DeleteDepartmentFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var department = this._databaseContext.Departments
            .Include(department => department.Users)
            .FirstOrDefault(department => department.Id == request.Id);
        if (department is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        foreach (var user in department.Users)
        {
            user.DepartmentId = null;
            user.DivisionId = null;
            user.RankId = null;
        }

        var departmentOldName = department.Name;
        this._databaseContext.Departments.Remove(department);
        this._databaseContext.SaveChanges();
        this.DepartmentDeleted?.Invoke(this, department);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.DELETE",
            "UI.LABEL.DEPARTMENT.SUCCESS.DELETE",
            new Dictionary<string, string>
            {
                { "department", departmentOldName },
            });
    }

    [UIEvent(Events.UI.Department.Join, true)]
    private void PlayerJoinDepartmentFromNui([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var department = this._databaseContext.Departments
            .Include(department => department.Users)
            .FirstOrDefault(department => department.Id == request.Id);

        if (department is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        try
        {
            this.AddPlayerToDepartment(player, department);
            var user = this._playerDataService.FindUserByLicense(player, false);
            if (department.DefaultRankId is not null)
            {
                user.RankId = department.DefaultRankId;
            }

            var callsignMatchesWithFormat = !string.IsNullOrEmpty(user.Callsign) && new Regex("^[A-Z0-9-*]+").IsMatch(user.Callsign);
            if (string.IsNullOrEmpty(user.Callsign) || !callsignMatchesWithFormat)
            {
                string callsign;
                do
                {
                    callsign = new CallsignGenerator(department.CallsignFormat).GenerateCallsign();
                }
                while (department.Users.Any(x => x.Callsign == callsign));

                user = this._playerDataService.FindUserByLicense(player);
                user.Callsign = callsign;
            }

            this._databaseContext.SaveChanges();

            networkCallbackDelegate.InvokeWithResponse(
                true,
                "UI.LABEL.SUCCESS.JOIN",
                "UI.LABEL.DEPARTMENT.SUCCESS.JOIN",
                new Dictionary<string, string>
                {
                    { "department", department!.Name },
                });
        }
        catch (DepartmentFullException)
        {
            networkCallbackDelegate.InvokeWithResponse(false, "UI.ERROR.DEPARTMENT.FULL.TITLE", "UI.ERROR.DEPARTMENT.FULL.DESCRIPTION");
        }
    }

    [UIEvent(Events.UI.Department.Leave, true)]
    private void PlayerLeaveDepartmentFromNui([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var playerDepartment = this._playerDataService.FindUserByLicense(player).Department;
        this.RemovePlayerFromDepartment(player);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.LEAVE",
            "UI.LABEL.DEPARTMENT.SUCCESS.LEAVE",
            new Dictionary<string, string>
            {
                { "department", playerDepartment!.Name },
            });
    }

    [UIEvent(Events.UI.Department.RemoveUser, true)]
    private void KickPlayerFromDepartmentNui([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var idRequest = expandoObject.ConvertTo<IdRequest>();
        var user = this._databaseContext.Users.FirstOrDefault(x => x.Id == idRequest.Id);
        if (user == null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }
        else
        {
            this.RemovePlayerFromDepartment(user);
        }

        networkCallbackDelegate.InvokeWithResponse(true, "Kick", "Kicking player");
    }

    [UIEvent(Events.UI.Department.GetAllSimpleWithArea)]
    private void GetAllSimpleWithAreaCoordinatesUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var departments = this._databaseContext.Departments
            .Include(x => x.AreaCoordinates)
            .Select(department => new SimpleDepartmentWithAreaDTO
            {
                Id = department.Id,
                Name = department.Name,
                Callsign = department.Callsign,
                Coordinates = department.AreaCoordinates.ToList(),
            })
            .ToList();
        networkCallbackDelegate.InvokeWithJson(departments);
    }

    [UIEvent(Events.UI.Department.UpdateAreas)]
    private void UpdateAreasUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<UpdateDepartmentAreasRequest>();
        foreach (var area in request.Areas)
        {
            var department = this._databaseContext.Departments
                .Include(x => x.AreaCoordinates)
                .FirstOrDefault(x => x.Id == area.DepartmentId);
            if (department is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            var coordinatesToRemove = department.AreaCoordinates
                .Where(areaCoordinate => area.Coordinates.All(x => x.Id != areaCoordinate.Id))
                .ToList();
            foreach (var coordinateToRemove in coordinatesToRemove)
            {
                this._databaseContext.DepartmentAreaCoordinates.Remove(coordinateToRemove);
            }

            foreach (var coordinate in area.Coordinates)
            {
                if (coordinate.Id == -1)
                {
                    department.AreaCoordinates.Add(new DepartmentAreaCoordinate
                    {
                        Department = department,
                        X = coordinate.X,
                        Y = coordinate.Y,
                    });
                }
                else
                {
                    var areaCoordinate = department.AreaCoordinates.FirstOrDefault(x => x.Id == coordinate.Id);
                    if (areaCoordinate is null)
                    {
                        continue;
                    }

                    areaCoordinate.X = coordinate.X;
                    areaCoordinate.Y = coordinate.Y;
                }
            }
        }

        this._databaseContext.SaveChanges();

        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.EDIT", "UI.LABEL.DEPARTMENT_AREA_MANAGEMENT.SUCCESS.EDIT");
    }
}