using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Extensions;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Exceptions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Events;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using FivePD.Server.Utilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc />
[Obfuscate]
public class DepartmentService : IDepartmentService
{
    private readonly DatabaseContext _databaseContext;
    private readonly IPlayerDataService _playerDataService;
    private readonly IPlayerHandlerService _playerHandlerService;
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DepartmentService"/> class.
    /// </summary>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    /// <param name="playerDataService">The <see cref="PlayerDataService"/> to use.</param>
    /// <param name="playerHandlerService">The <see cref="PlayerHandlerService"/> to use.</param>
    public DepartmentService(DatabaseContext databaseContext, IPlayerDataService playerDataService, IPlayerHandlerService playerHandlerService, INetworkEventService networkEventService)
    {
        this._databaseContext = databaseContext;
        this._playerDataService = playerDataService;
        this._playerHandlerService = playerHandlerService;
        this._networkEventService = networkEventService;
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

        var departmentExists = this._databaseContext.Departments.FirstOrDefault(d => d.Name == department.Name && d.Id != department.Id);
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
    public Department UpdateDepartment(Department department)
    {
        if (department is null)
        {
            throw new ArgumentException("The provided department is null");
        }
        
        var departmentEntity = this._databaseContext.Departments.FirstOrDefault(d => d.Name == department.Name);
        if (departmentEntity is null)
        {
            throw new ArgumentException("The provided department does not exist");
        }
        
        if (string.IsNullOrEmpty(department.Name))
        {
            throw new ArgumentException("The name of the department can't be null or empty");
        }
        
        var departmentExists = this._databaseContext.Departments.FirstOrDefault(d => d.Name == department.Name && d.Id != department.Id);
        if (departmentExists is not null)
        {
            throw new DepartmentExistsWithNameException();
        }
        
        departmentEntity.Name = department.Name;
        departmentEntity.DepartmentRanks = department.DepartmentRanks;
        departmentEntity.Divisions = department.Divisions;
        departmentEntity.Callsign = department.Callsign;
        departmentEntity.CalloutDetailsDepartments = department.CalloutDetailsDepartments;
        departmentEntity.AreaCoordinates = department.AreaCoordinates;
        departmentEntity.Users = department.Users;
        departmentEntity.CallsignFormat = department.CallsignFormat;
        departmentEntity.DefaultRank = department.DefaultRank;
        departmentEntity.MaxUsers = department.MaxUsers;
        departmentEntity.UseGlobalRanks = department.UseGlobalRanks;

        this._databaseContext.SaveChanges();
        this.DepartmentUpdated?.Invoke(this, departmentEntity);
        return departmentEntity;
    }

    /// <inheritdoc />
    public void DeleteDepartment(Department department)
    {
        this._databaseContext.Departments.Remove(department);
        this._databaseContext.SaveChanges();
        this.DepartmentDeleted?.Invoke(this, department);
    }
    
     /// <inheritdoc />
    public Division CreateDivision(Division division)
    {
        if (division is null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(division.Name))
        {
            return null;
        }

        var divisionExists = this._databaseContext.Divisions.FirstOrDefault(d => d.Name == division.Name);
        if (divisionExists is not null)
        {
            throw new DivisionExistsWithNameException();
        }

        var divisionEntry = this._databaseContext.Divisions.Add(division);
        this._databaseContext.SaveChanges();
        this.DivisionCreated?.Invoke(this, divisionEntry.Entity);
        return divisionEntry.Entity;
    }

    /// <inheritdoc />
    public Division UpdateDivision(Division division)
    {
        if (division is null)
        {
            return null;
        }
        
        var divisionEntity = this._databaseContext.Divisions.FirstOrDefault(d => d.Name == division.Name);
        if (divisionEntity is null)
        {
            return null;
        }
        
        if (string.IsNullOrEmpty(division.Name))
        {
            throw new ArgumentException("The name of the division can't be null or empty");
        }
        
        var divisionExists = this._databaseContext.Divisions.FirstOrDefault(d => d.Name == division.Name && d.Id != division.Id);
        if (divisionExists is not null)
        {
            throw new DivisionExistsWithNameException();
        }
        
        divisionEntity.Name = division.Name;
        divisionEntity.Users = division.Users;
        divisionEntity.Department = division.Department;

        this._databaseContext.SaveChanges();
        this.DivisionUpdated?.Invoke(this, divisionEntity);
        return divisionEntity;
    }

    /// <inheritdoc />
    public void DeleteDivision(Division division)
    {
        this._databaseContext.Divisions.Remove(division);
        this._databaseContext.SaveChanges();
        this.DivisionDeleted?.Invoke(this, division);
    }

    /// <inheritdoc />
    public IEnumerable<Department> GetDepartments(DepartmentQueryOptions options = default)
    {
        return this._databaseContext.Departments
            .IncludeIf(options.IncludeDivisions, dept => dept.Divisions)
            .IncludeIf(options.IncludeRanks, dept => dept.DepartmentRanks)
            .IncludeIf(options.IncludeUsers, dept => dept.Users);
    }

    /// <inheritdoc />
    public Department GetDepartmentById(int id, DepartmentQueryOptions options = default)
    {
        return this._databaseContext.Departments
            .IncludeIf(options.IncludeDivisions, dept => dept.Divisions)
            .IncludeIf(options.IncludeRanks, dept => dept.DepartmentRanks)
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
    public void RemovePlayerFromDepartment(FPlayer player)
    {
        this.RemovePlayerFromDepartment(this._playerDataService.FindUserByLicense(player.GetUnderlyingPlayer()));
    }

    /// <inheritdoc />
    public Department GetPlayerDepartment(Player player)
    {
        return this.GetPlayerDepartment(FPlayer.FromPlayer(player));
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
    private void GetAllUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var departments = this._databaseContext.Departments
            .AsNoTracking()
            .Select(x => new SimpleDepartmentDTO
            {
                Id = x.Id,
                Name = x.Name,
            });
        this._networkEventService.TriggerEventSerialized(player, guid, departments);
    }

    [UIEvent(Events.UI.Department.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var departmentsRequest = JsonConvert.DeserializeObject<DepartmentsRequest>(serializedRequest);
        User user = this._playerHandlerService.FindUserOfPlayer(player);
        var filters = new Filters<Department>();
        filters.Add(
            !string.IsNullOrEmpty(departmentsRequest.Filters.Name),
            department => department.Name.ToLower().Contains(departmentsRequest.Filters.Name.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(departmentsRequest.Filters.Callsign),
            department => department.Callsign.ToLower().Contains(departmentsRequest.Filters.Callsign.ToLower()));
        filters.Add(
            departmentsRequest.Filters.UserDepartment,
            department => department.Id == user.DepartmentId);
        var page = this._databaseContext.Departments
            .AsNoTracking()
            .Include(department => department.Users)
            .Paginate(departmentsRequest.Page, filters);

        page.Items = page.Items.Select(item => new DepartmentResponse(item)).ToList();
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.Department.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var department = this._databaseContext.Departments
            .AsNoTracking()
            .Include(dept => dept.Divisions)
            .Include(dept => dept.DepartmentRanks)
            .ThenInclude(x => x.Rank)
            .FirstOrDefault(dept => dept.Id == request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, department);
    }

    [UIEvent(Events.UI.Department.GetUsers)]
    private void GetDepartmentUsersForNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var departmentUsersRequest = JsonConvert.DeserializeObject<DepartmentUsersRequest>(serializedRequest);

        var filters = new Filters<User>();
        filters.Add(true, user => user.Department.Id == departmentUsersRequest.DepartmentId);
        filters.Add(
            !string.IsNullOrEmpty(departmentUsersRequest.Filters.Name),
            department => department.Name.ToLower().Contains(departmentUsersRequest.Filters.Name.ToLower()));

        var page = this._databaseContext.Users.AsNoTracking().Include(x => x.CallsignFormat).Paginate(departmentUsersRequest.Page, filters);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.Department.GetDivision)]
    private void GetDepartmentDivisionForNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);

        var division = this._databaseContext.Divisions
            .AsNoTracking()
            .FirstOrDefault(division => division.Id == request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, division);
    }

    [UIEvent(Events.UI.Department.GetDivisions)]
    private void GetDepartmentDivisionsForNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<DepartmentDivisionsRequest>(serializedRequest);

        var filters = new Filters<Division>();
        filters.Add(true, division => division.Department.Id == request.DepartmentId);
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Name),
            department => department.Name.ToLower().Contains(request.Filters.Name.ToLower()));

        var page = this._databaseContext.Divisions
            .AsNoTracking()
            .Paginate(request.Page, filters);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.Department.Create)]
    private void CreateDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        try
        {
            var request = JsonConvert.DeserializeObject<CreateDepartmentRequest>(serializedRequest);
            this.CreateDepartment(new Department
            {
                Name = request.Name,
                CallsignFormat = request.CallsignFormat,
                MaxUsers = request.MaxUsers,
            });
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                true,
                "UI.LABEL.SUCCESS.CREATE",
                "UI.LABEL.DEPARTMENT.SUCCESS.CREATE",
                new Dictionary<string, string>
                {
                    { "department", request.Name },
                });
        }
        catch (DepartmentExistsWithNameException)
        {
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                false,
                "UI.ERROR.DEPARTMENT.CREATE.TITLE",
                "UI.ERROR.DEPARTMENT.NAME_CONFLICT.DESCRIPTION");
        }
    }

    [UIEvent(Events.UI.Department.DeleteDivision)]
    private void DeleteDivisionFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var divisionToDelete = this._databaseContext.Divisions
            .Include(division => division.Users)
            .Include(division => division.Department)
            .FirstOrDefault(division => division.Id == request.Id);
        if (divisionToDelete is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
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
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "UI.LABEL.SUCCESS.DELETE",
            "UI.LABEL.DIVISION.SUCCESS.DELETE",
            new Dictionary<string, string>
            {
                { "division", divisionToDeleteName },
            });
    }

    [UIEvent(Events.UI.Department.EditDivision)]
    private void EditDivisionFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditDivisionRequest>(serializedRequest);
        var divisionToEdit =
            this._databaseContext.Divisions.FirstOrDefault(division =>
                division.Id == request.DivisionId);
        if (divisionToEdit is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var divisionExistsWithName = this._databaseContext.Divisions
            .FirstOrDefault(division => division.Name == request.Name &&
                                        division.Department.Id == request.DepartmentId &&
                                        division.Id != divisionToEdit.Id);
        if (divisionExistsWithName != null)
        {
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                false,
                "UI.ERROR.DIVISION.EDIT.TITLE",
                "UI.ERROR.DIVISION.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var divisionToEditOldName = divisionToEdit.Name;
        divisionToEdit.Name = request.Name;
        this._databaseContext.SaveChanges();
        this.DivisionUpdated?.Invoke(this, divisionToEdit);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "UI.LABEL.SUCCESS.EDIT",
            "UI.LABEL.DIVISION.SUCCESS.EDIT",
            new Dictionary<string, string>
            {
                { "division", divisionToEditOldName },
            });
    }

    [UIEvent(Events.UI.Department.CreateDivision)]
    private void CreateDivisionFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CreateDivisionRequest>(serializedRequest);
        var divisionExistsWithName = this._databaseContext.Divisions.FirstOrDefault(division =>
            division.Name == request.Name && division.Department.Id == request.DepartmentId);
        if (divisionExistsWithName != null)
        {
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                false,
                "UI.ERROR.DIVISION.CREATE.TITLE",
                "UI.ERROR.DIVISION.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        var department = this._databaseContext.Departments.FirstOrDefault(department => department.Id == request.DepartmentId);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var createdDivisionEntry = this._databaseContext.Divisions.Add(new Division
        {
            Department = department,
            Name = request.Name,
        });
        this._databaseContext.SaveChanges();
        this.DivisionCreated?.Invoke(this, createdDivisionEntry.Entity);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "UI.LABEL.SUCCESS.CREATE",
            "UI.LABEL.DIVISION.SUCCESS.CREATE",
            new Dictionary<string, string>
            {
                { "division", request.Name },
            });
    }

    [UIEvent(Events.UI.Department.Edit)]
    private void EditDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditDepartmentRequest>(serializedRequest);
        var department = this._databaseContext.Departments
            .Include(department => department.Users)
            .FirstOrDefault(department => department.Id == request.DepartmentId);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var departmentExistsWithName = this._databaseContext.Departments.AsNoTracking().FirstOrDefault(dept =>
            dept.Id != request.DepartmentId && dept.Name == request.Name);
        if (departmentExistsWithName != null)
        {
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                false,
                "UI.ERROR.DEPARTMENT.EDIT.TITLE",
                "UI.ERROR.DEPARTMENT.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        if (request.DefaultRankId != -1)
        {
            var defaultRank = this._databaseContext.Ranks.FirstOrDefault(rank => rank.Id == request.DefaultRankId);
            if (defaultRank is null)
            {
                this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
                return;
            }
        }

        var departmentOldName = department.Name;
        department.Name = request.Name;
        department.Callsign = string.IsNullOrEmpty(request.Callsign)
            ? request.Name.ToAcronym()
            : request.Callsign;
        department.MaxUsers = request.MaxUsers;
        department.DefaultRankId = request.DefaultRankId == -1 ? (int?)null : request.DefaultRankId;

        var oldUseGlobalRanks = department.UseGlobalRanks;
        department.UseGlobalRanks = request.UseGlobalRanks;

        if (oldUseGlobalRanks != request.UseGlobalRanks)
        {
            foreach (var user in department.Users)
            {
                user.RankId = null;
            }
        }

        this._databaseContext.SaveChanges();
        this.DepartmentUpdated?.Invoke(this, department);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "UI.LABEL.SUCCESS.EDIT",
            "UI.LABEL.DEPARTMENT.SUCCESS.EDIT",
            new Dictionary<string, string>
            {
                { "department", departmentOldName },
            });
    }

    [UIEvent(Events.UI.Department.EditCallsignFormat)]
    private void EditDepartmentCallsignFormatUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditDepartmentCallsignFormatRequest>(serializedRequest);
        var department = this._databaseContext.Departments
            .Include(x => x.Users)
            .ThenInclude(u => u.CallsignFormat)
            .FirstOrDefault(department => department.Id == request.DepartmentId);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        department.CallsignFormat = request.CallsignFormat;

        foreach (var user in department.Users)
        {
            var callsignGenerator = new CallsignGenerator(department.CallsignFormat);
            string callsign;
            do
            {
                callsign = callsignGenerator.GenerateCallsign().BuildCallsign();
            }
            while (department.Users.Any(x => x.Callsign == callsign));

            user.Callsign = callsign;
            if (user.CallsignFormat is not null)
            {
                user.CallsignFormat.Values = callsignGenerator.GetGeneratedItemValues();
            }
            else
            {
                var entry = this._databaseContext.UserCallsigns.Add(new UserCallsignFormat
                {
                    Values = callsignGenerator.GetGeneratedItemValues(),
                });
                this._databaseContext.SaveChanges();
                user.UserCallsignFormatId = entry.Entity.Id;
            }
        }

        this._databaseContext.SaveChanges();
        this.DepartmentUpdated?.Invoke(this, department);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "UI.LABEL.SUCCESS.EDIT",
            "UI.LABEL.DEPARTMENT.SUCCESS.EDIT",
            new Dictionary<string, string>
            {
                { "department", department.Name },
            });
    }

    [UIEvent(Events.UI.Department.CheckIfDepartmentNameTaken)]
    private void CheckIfDepartmentNameTaken([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<NameTakenRequest>(serializedRequest);
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

        this._networkEventService.TriggerEventSerialized(player, guid, new NameTakenResponse(departmentExists != null));
    }

    [UIEvent(Events.UI.Department.Delete)]
    private void DeleteDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var department = this._databaseContext.Departments
            .Include(department => department.Users)
            .FirstOrDefault(department => department.Id == request.Id);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
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
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "UI.LABEL.SUCCESS.DELETE",
            "UI.LABEL.DEPARTMENT.SUCCESS.DELETE",
            new Dictionary<string, string>
            {
                { "department", departmentOldName },
            });
    }

    [UIEvent(Events.UI.Department.Join)]
    private void PlayerJoinDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var department = this._databaseContext.Departments
            .Include(department => department.Users)
            .ThenInclude(u => u.CallsignFormat)
            .FirstOrDefault(department => department.Id == request.Id);

        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
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

            var callsign = string.Join(string.Empty, user.Callsign);
            var callsignMatchesWithFormat = !string.IsNullOrEmpty(callsign) && new Regex("^[A-Z0-9-]+$").IsMatch(callsign);
            if (string.IsNullOrEmpty(callsign) || !callsignMatchesWithFormat)
            {
                var callsignGenerator = new CallsignGenerator(department.CallsignFormat);
                string newCallsign;
                do
                {
                    newCallsign = callsignGenerator.GenerateCallsign().BuildCallsign();
                }
                while (department.Users.Any(x => x.Callsign == newCallsign));

                user = this._playerDataService.FindUserByLicense(player);
                user.Callsign = newCallsign;

                if (user.CallsignFormat is not null)
                {
                    user.CallsignFormat.Values = callsignGenerator.GetGeneratedItemValues();
                }
                else
                {
                    var entry = this._databaseContext.UserCallsigns.Add(new UserCallsignFormat
                    {
                        Values = callsignGenerator.GetGeneratedItemValues(),
                    });
                    this._databaseContext.SaveChanges();
                    user.UserCallsignFormatId = entry.Entity.Id;
                }
            }

            this._databaseContext.SaveChanges();

            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
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
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "UI.ERROR.DEPARTMENT.FULL.TITLE", "UI.ERROR.DEPARTMENT.FULL.DESCRIPTION");
        }
    }

    [UIEvent(Events.UI.Department.Leave)]
    private void PlayerLeaveDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var playerDepartment = this._playerDataService.FindUserByLicense(player).Department;
        this.RemovePlayerFromDepartment(player);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "UI.LABEL.SUCCESS.LEAVE",
            "UI.LABEL.DEPARTMENT.SUCCESS.LEAVE",
            new Dictionary<string, string>
            {
                { "department", playerDepartment!.Name },
            });
    }

    [UIEvent(Events.UI.Department.RemoveUser)]
    private void KickPlayerFromDepartmentNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var idRequest = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var user = this._databaseContext.Users.FirstOrDefault(x => x.Id == idRequest.Id);
        if (user is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this.RemovePlayerFromDepartment(user);

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "Kick", "Kicking player");
    }

    [UIEvent(Events.UI.Department.GetAllSimpleWithArea)]
    private void GetAllSimpleWithAreaCoordinatesUI([FromSource] Player player, string guid, string serializedRequest)
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
        this._networkEventService.TriggerEventSerialized(player, guid, departments);
    }

    [UIEvent(Events.UI.Department.UpdateAreas)]
    private void UpdateAreasUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<UpdateDepartmentAreasRequest>(serializedRequest);
        foreach (var area in request.Areas)
        {
            var department = this._databaseContext.Departments
                .Include(x => x.AreaCoordinates)
                .FirstOrDefault(x => x.Id == area.DepartmentId);
            if (department is null)
            {
                this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
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

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "UI.LABEL.SUCCESS.EDIT", "UI.LABEL.DEPARTMENT_AREA_MANAGEMENT.SUCCESS.EDIT");
    }
}