using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CitizenFX.Core;
using Dapper;
using FivePD.Common;
using FivePD.Common.Extensions;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Exceptions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Events;
using FivePD.Server.Models.Requests;
using FivePD.Server.Repositories;
using FivePD.Server.Utilities;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc />
[Obfuscate]
public class DepartmentService : IDepartmentService
{
    private readonly DepartmentRepository _departmentRepository;
    private readonly UserRepository _userRepository;
    private readonly DivisionRepository _divisionRepository;
    private readonly RankRepository _rankRepository;
    private readonly IPlayerDataService _playerDataService;
    private readonly IPlayerHandlerService _playerHandlerService;
    private readonly INetworkEventService _networkEventService;
    private readonly IDapperDatabaseConnectionFactory _dbConnectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DepartmentService"/> class.
    /// </summary>
    /// <param name="departmentRepository">The <see cref="DepartmentRepository"/> to use.</param>
    /// <param name="userRepository">The <see cref="UserRepository"/> to use.</param>
    /// <param name="divisionRepository">The <see cref="DivisionRepository"/> to use.</param>
    /// <param name="rankRepository">The <see cref="RankRepository"/> to use.</param>
    /// <param name="playerDataService">The <see cref="PlayerDataService"/> to use.</param>
    /// <param name="playerHandlerService">The <see cref="PlayerHandlerService"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public DepartmentService(
        DepartmentRepository departmentRepository,
        UserRepository userRepository,
        DivisionRepository divisionRepository,
        RankRepository rankRepository,
        IPlayerDataService playerDataService,
        IPlayerHandlerService playerHandlerService,
        INetworkEventService networkEventService,
        IDapperDatabaseConnectionFactory dbConnectionFactory)
    {
        this._departmentRepository = departmentRepository;
        this._userRepository = userRepository;
        this._divisionRepository = divisionRepository;
        this._rankRepository = rankRepository;
        this._playerDataService = playerDataService;
        this._playerHandlerService = playerHandlerService;
        this._networkEventService = networkEventService;
        this._dbConnectionFactory = dbConnectionFactory;
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

    [UIEvent(Events.UI.Department.GetAll)]
    private void GetAllUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var departments = this._departmentRepository
            .GetAll()
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
        var request = JsonConvert.DeserializeObject<DepartmentsRequest>(serializedRequest);
        var user = this._playerHandlerService.FindUserOfPlayer(player);
        var sqlBuilder = new SqlBuilder()
            .If(!string.IsNullOrEmpty(request.Filters.Name), b => b.WhereContains("Name", request.Filters.Name))
            .If(!string.IsNullOrEmpty(request.Filters.Callsign), b => b.WhereContains("Callsign", request.Filters.Callsign))
            .If(request.Filters.UserDepartment, b => b.Where("Id = @Id", new { Id = user.DepartmentId }));
        var page = this._departmentRepository.Paginate(request.Page, sqlBuilder);
        if (!page.Items.Any())
        {
            this._networkEventService.TriggerEventSerialized(player, guid, page);
            return;
        }

        var (sql, parameters) = new SqlBuilder(TableNames.Departments)
            .LeftJoin(TableNames.Users, "u", $"{TableNames.Departments}.Id = u.DepartmentId")
            .WhereIn($"{TableNames.Departments}.Id", page.Items.Select(d => d.Id))
            .Select($"{TableNames.Departments}.Id as DepartmentId, COUNT(u.Id) as UserCount")
            .GroupBy($"{TableNames.Departments}.Id")
            .Build();
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var departmentUsersCount = connection.Query<DepartmentUserCountDto>(sql, parameters).ToDictionary(c => c.DepartmentId);
        page.Items = page.Items.Select(item => new DepartmentResponse(item, departmentUsersCount[item.Id]?.UserCount ?? 0)).ToList();
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.Department.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var department = this._departmentRepository.FindById(request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, department);
    }

    [UIEvent(Events.UI.Department.GetUsers)]
    private void GetDepartmentUsersForNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<DepartmentUsersRequest>(serializedRequest);
        var sqlBuilder = new SqlBuilder()
            .If(true, b => b.Where("DepartmentId = @DepartmentId", new { DepartmentId = request.DepartmentId }))
            .If(!string.IsNullOrEmpty(request.Filters.Name), b => b.WhereContains("Name", request.Filters.Name));
        var page = this._userRepository.Paginate(request.Page, sqlBuilder);
        if (!page.Items.Any())
        {
            this._networkEventService.TriggerEventSerialized(player, guid, page);
            return;
        }

        var (sql, parameters) = new SqlBuilder(TableNames.UserCallsigns)
            .InnerJoin(TableNames.Users, "u", $"u.UserCallsignFormatId = {TableNames.UserCallsigns}.Id")
            .WhereIn("u.Id", page.Items.Select(d => d.Id))
            .Build();
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var callsigns = connection.Query<UserCallsignFormat>(sql, parameters).ToDictionary(c => c.Id);
        foreach (var user in page.Items)
        {
            if (user.UserCallsignFormatId is not null)
            {
                user.CallsignFormat = callsigns[user.UserCallsignFormatId ?? 0];
            }
        }

        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.Department.GetDivision)]
    private void GetDepartmentDivisionForNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var division = this._divisionRepository.FindById(request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, division);
    }

    [UIEvent(Events.UI.Department.GetDivisions)]
    private void GetDepartmentDivisionsForNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<DepartmentDivisionsRequest>(serializedRequest);
        var sqlBuilder = new SqlBuilder()
            .If(true, b => b.Where("DepartmentId = @DepartmentId", new { DepartmentId = request.DepartmentId }))
            .If(!string.IsNullOrEmpty(request.Filters.Name), b => b.WhereContains("Name", request.Filters.Name));
        var page = this._divisionRepository.Paginate(request.Page, sqlBuilder);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.Department.Create)]
    private void CreateDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CreateDepartmentRequest>(serializedRequest);
        var (sql, parameters) = new SqlBuilder(TableNames.Departments)
            .Where("Name = @Name", new { Name = request.Name })
            .Build(isCount: true);
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var departmentWithSameName = connection.ExecuteScalar<int>(sql, parameters);
        if (departmentWithSameName > 0)
        {
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                false,
                "ERROR.DEPARTMENT.CREATE.TITLE",
                "ERROR.DEPARTMENT.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        this._departmentRepository.Add(new Department
        {
            Name = request.Name,
            CallsignFormat = request.CallsignFormat,
            MaxUsers = request.MaxUsers,
        });
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.CREATE",
            "LABEL.DEPARTMENT.SUCCESS.CREATE",
            new Dictionary<string, string>
            {
                { "department", request.Name },
            });
    }

    [UIEvent(Events.UI.Department.DeleteDivision)]
    private void DeleteDivisionFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var division = this._divisionRepository.FindById(request.Id);
        if (division is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._divisionRepository.Delete(request.Id);
        this.DivisionDeleted?.Invoke(this, division);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.DELETE",
            "LABEL.DIVISION.SUCCESS.DELETE",
            new Dictionary<string, string>
            {
                { "division", division.Name },
            });
    }

    [UIEvent(Events.UI.Department.EditDivision)]
    private void EditDivisionFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditDivisionRequest>(serializedRequest);
        var division = this._divisionRepository.FindById(request.DivisionId);
        if (division is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var (sql, parameters) = new SqlBuilder(TableNames.Divisions)
            .Where("Name = @Name", new { Name = request.Name })
            .Where("DepartmentId = @DepartmentId", new { DepartmentId = request.DepartmentId })
            .Build(isCount: true);
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var divisionWithSameName = connection.ExecuteScalar<int>(sql, parameters);
        if (divisionWithSameName > 0)
        {
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                false,
                "ERROR.DIVISION.EDIT.TITLE",
                "ERROR.DIVISION.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        this._divisionRepository.Update(division, x => { x.Name = request.Name; });
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.EDIT",
            "LABEL.DIVISION.SUCCESS.EDIT",
            new Dictionary<string, string>
            {
                { "division", division.Name },
            });
    }

    [UIEvent(Events.UI.Department.CreateDivision)]
    private void CreateDivisionFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CreateDivisionRequest>(serializedRequest);
        var (sql, parameters) = new SqlBuilder(TableNames.Divisions)
            .Where("Name = @Name", new { Name = request.Name })
            .Where("DepartmentId = @DepartmentId", new { DepartmentId = request.DepartmentId })
            .Build(isCount: true);
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var divisionWithSameName = connection.ExecuteScalar<int>(sql, parameters);
        if (divisionWithSameName > 0)
        {
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                false,
                "ERROR.DIVISION.CREATE.TITLE",
                "ERROR.DIVISION.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        if (!this._departmentRepository.Exists(request.DepartmentId))
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._divisionRepository.Add(new Division
        {
            DepartmentId = request.DepartmentId,
            Name = request.Name,
        });
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.CREATE",
            "LABEL.DIVISION.SUCCESS.CREATE",
            new Dictionary<string, string>
            {
                { "division", request.Name },
            });
    }

    [UIEvent(Events.UI.Department.Edit)]
    private void EditDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditDepartmentRequest>(serializedRequest);
        var department = this._departmentRepository.FindById(request.DepartmentId);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var (sql, parameters) = new SqlBuilder(TableNames.Departments)
            .Where("Name = @Name", new { Name = request.Name })
            .Where("DepartmentId != @DepartmentId", new { DepartmentId = request.DepartmentId })
            .Build(isCount: true);
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var departmentWithSameName = connection.ExecuteScalar<int>(sql, parameters);
        if (departmentWithSameName > 0)
        {
            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                false,
                "ERROR.DEPARTMENT.EDIT.TITLE",
                "ERROR.DEPARTMENT.NAME_CONFLICT.DESCRIPTION");
            return;
        }

        if (request.DefaultRankId != -1 && !this._rankRepository.Exists(request.DefaultRankId))
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._departmentRepository.Update(department, x =>
        {
            x.Name = request.Name;
            x.Callsign = string.IsNullOrEmpty(request.Callsign)
                ? request.Name.ToAcronym()
                : request.Callsign;
            x.MaxUsers = request.MaxUsers;
            x.DefaultRankId = request.DefaultRankId == -1 ? null : request.DefaultRankId;
        });

        var oldUseGlobalRanks = department.UseGlobalRanks;
        department.UseGlobalRanks = request.UseGlobalRanks;

        if (oldUseGlobalRanks != request.UseGlobalRanks)
        {
            var users = this._departmentRepository.GetUsers(department);
            foreach (var user in users)
            {
                this._userRepository.Update(user, x => { x.RankId = null; });
            }
        }

        this.DepartmentUpdated?.Invoke(this, department);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.EDIT",
            "LABEL.DEPARTMENT.SUCCESS.EDIT",
            new Dictionary<string, string>
            {
                { "department", department.Name },
            });
    }

    [UIEvent(Events.UI.Department.EditCallsignFormat)]
    private void EditDepartmentCallsignFormatUI([FromSource] Player player, string guid, string serializedRequest)
    {
        // using var databaseContext = this._dbContextFactory.CreateScope().Context;
        // var request = JsonConvert.DeserializeObject<EditDepartmentCallsignFormatRequest>(serializedRequest);
        // var department = databaseContext.Departments
        //     .Include(x => x.Users)
        //     .ThenInclude(u => u.CallsignFormat)
        //     .FirstOrDefault(department => department.Id == request.DepartmentId);
        // if (department is null)
        // {
        //     this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
        //     return;
        // }
        //
        // department.CallsignFormat = request.CallsignFormat;
        //
        // foreach (var user in department.Users)
        // {
        //     var callsignGenerator = new CallsignGenerator(department.CallsignFormat);
        //     string callsign;
        //     do
        //     {
        //         callsign = callsignGenerator.GenerateCallsign().BuildCallsign();
        //     }
        //     while (department.Users.Any(x => x.Callsign == callsign));
        //
        //     user.Callsign = callsign;
        //     if (user.CallsignFormat is not null)
        //     {
        //         user.CallsignFormat.Values = callsignGenerator.GetGeneratedItemValues();
        //     }
        //     else
        //     {
        //         var entry = databaseContext.UserCallsigns.Add(new UserCallsignFormat
        //         {
        //             Values = callsignGenerator.GetGeneratedItemValues(),
        //         });
        //         databaseContext.SaveChanges();
        //         user.UserCallsignFormatId = entry.Entity.Id;
        //     }
        // }
        //
        // databaseContext.SaveChanges();
        // this.DepartmentUpdated?.Invoke(this, department);
        // this._networkEventService.TriggerEventWithResponse(
        //     player,
        //     guid,
        //     true,
        //     "LABEL.SUCCESS.EDIT",
        //     "LABEL.DEPARTMENT.SUCCESS.EDIT",
        //     new Dictionary<string, string>
        //     {
        //         { "department", department.Name },
        //     });
    }

    [UIEvent(Events.UI.Department.CheckIfDepartmentNameTaken)]
    private void CheckIfDepartmentNameTaken([FromSource] Player player, string guid, string serializedRequest)
    {
        // using var databaseContext = this._dbContextFactory.CreateScope().Context;
        // var request = JsonConvert.DeserializeObject<NameTakenRequest>(serializedRequest);
        // Department departmentExists;
        // if (request.CheckForDepartmentId)
        // {
        //     departmentExists =
        //         databaseContext.Departments.FirstOrDefault(department =>
        //             department.Name == request.Name && department.Id != request.DepartmentId);
        // }
        // else
        // {
        //     departmentExists = databaseContext.Departments.FirstOrDefault(department => department.Name == request.Name);
        // }
        //
        // this._networkEventService.TriggerEventSerialized(player, guid, new NameTakenResponse(departmentExists != null));
    }

    [UIEvent(Events.UI.Department.Delete)]
    private void DeleteDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        // using var databaseContext = this._dbContextFactory.CreateScope().Context;
        // var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        // var department = databaseContext.Departments
        //     .Include(department => department.Users)
        //     .FirstOrDefault(department => department.Id == request.Id);
        // if (department is null)
        // {
        //     this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
        //     return;
        // }
        //
        // foreach (var user in department.Users)
        // {
        //     user.DepartmentId = null;
        //     user.DivisionId = null;
        //     user.RankId = null;
        // }
        //
        // var departmentOldName = department.Name;
        // databaseContext.Departments.Remove(department);
        // databaseContext.SaveChanges();
        // this.DepartmentDeleted?.Invoke(this, department);
        // this._networkEventService.TriggerEventWithResponse(
        //     player,
        //     guid,
        //     true,
        //     "LABEL.SUCCESS.DELETE",
        //     "LABEL.DEPARTMENT.SUCCESS.DELETE",
        //     new Dictionary<string, string>
        //     {
        //         { "department", departmentOldName },
        //     });
    }

    [UIEvent(Events.UI.Department.Join)]
    private void PlayerJoinDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var department = this._departmentRepository.FindById(request.Id);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        try
        {
            var user = this._playerDataService.FindUserByLicense(player, false);
            this._userRepository.Update(user, x => { x.DepartmentId = department.Id; });
            if (department.DefaultRankId is not null)
            {
                this._userRepository.Update(user, x => { x.RankId = department.DefaultRankId; });
            }

            var callsign = string.Join(string.Empty, user.Callsign);
            var callsignMatchesWithFormat = !string.IsNullOrEmpty(callsign) && new Regex("^[A-Z0-9-]+$").IsMatch(callsign);
            if (string.IsNullOrEmpty(callsign) || !callsignMatchesWithFormat)
            {
                var callsignGenerator = new CallsignGenerator(department.CallsignFormat);
                var departmentUsers = this._departmentRepository.GetUsers(department).ToList();
                string newCallsign;
                do
                {
                    newCallsign = callsignGenerator.GenerateCallsign().BuildCallsign();
                }
                while (departmentUsers.Any(x => x.Callsign == newCallsign));

                user = this._playerDataService.FindUserByLicense(player);
                this._userRepository.Update(user, x => { x.Callsign = newCallsign; });

                if (user.CallsignFormat is not null)
                {
                    user.CallsignFormat.Values = callsignGenerator.GetGeneratedItemValues();
                }
                else
                {
                    var callsignFormat = this._userRepository.AddCallsignFormat(user, new UserCallsignFormat
                    {
                        Values = callsignGenerator.GetGeneratedItemValues(),
                    });
                    this._userRepository.Update(user, x => { x.UserCallsignFormatId = callsignFormat.Id; });
                }
            }

            this._networkEventService.TriggerEventWithResponse(
                player,
                guid,
                true,
                "LABEL.SUCCESS.JOIN",
                "LABEL.DEPARTMENT.SUCCESS.JOIN",
                new Dictionary<string, string>
                {
                    { "department", department!.Name },
                });
        }
        catch (DepartmentFullException)
        {
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "ERROR.DEPARTMENT.FULL.TITLE", "ERROR.DEPARTMENT.FULL.DESCRIPTION");
        }
    }

    [UIEvent(Events.UI.Department.Leave)]
    private void PlayerLeaveDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        // var playerDepartment = this._playerDataService.FindUserByLicense(player).Department;
        // this.RemovePlayerFromDepartment(player);
        // this._networkEventService.TriggerEventWithResponse(
        //     player,
        //     guid,
        //     true,
        //     "LABEL.SUCCESS.LEAVE",
        //     "LABEL.DEPARTMENT.SUCCESS.LEAVE",
        //     new Dictionary<string, string>
        //     {
        //         { "department", playerDepartment!.Name },
        //     });
    }

    [UIEvent(Events.UI.Department.RemoveUser)]
    private void KickPlayerFromDepartmentNui([FromSource] Player player, string guid, string serializedRequest)
    {
        // using var databaseContext = this._dbContextFactory.CreateScope().Context;
        // var idRequest = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        // var user = databaseContext.Users.FirstOrDefault(x => x.Id == idRequest.Id);
        // if (user is null)
        // {
        //     this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
        //     return;
        // }
        //
        // this.RemovePlayerFromDepartment(user);
        //
        // this._networkEventService.TriggerEventWithResponse(player, guid, true, "Kick", "Kicking player");
    }

    [UIEvent(Events.UI.Department.GetAllSimpleWithArea)]
    private void GetAllSimpleWithAreaCoordinatesUI([FromSource] Player player, string guid, string serializedRequest)
    {
        // using var databaseContext = this._dbContextFactory.CreateScope().Context;
        // var departments = databaseContext.Departments
        //     .Include(x => x.AreaCoordinates)
        //     .Select(department => new SimpleDepartmentWithAreaDTO
        //     {
        //         Id = department.Id,
        //         Name = department.Name,
        //         Callsign = department.Callsign,
        //         Coordinates = department.AreaCoordinates.ToList(),
        //     })
        //     .ToList();
        // this._networkEventService.TriggerEventSerialized(player, guid, departments);
    }

    [UIEvent(Events.UI.Department.UpdateAreas)]
    private void UpdateAreasUI([FromSource] Player player, string guid, string serializedRequest)
    {
        // using var databaseContext = this._dbContextFactory.CreateScope().Context;
        // var request = JsonConvert.DeserializeObject<UpdateDepartmentAreasRequest>(serializedRequest);
        // foreach (var area in request.Areas)
        // {
        //     var department = databaseContext.Departments
        //         .Include(x => x.AreaCoordinates)
        //         .FirstOrDefault(x => x.Id == area.DepartmentId);
        //     if (department is null)
        //     {
        //         this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
        //         return;
        //     }
        //
        //     var coordinatesToRemove = department.AreaCoordinates
        //         .Where(areaCoordinate => area.Coordinates.All(x => x.Id != areaCoordinate.Id))
        //         .ToList();
        //     foreach (var coordinateToRemove in coordinatesToRemove)
        //     {
        //         databaseContext.DepartmentAreaCoordinates.Remove(coordinateToRemove);
        //     }
        //
        //     foreach (var coordinate in area.Coordinates)
        //     {
        //         if (coordinate.Id == -1)
        //         {
        //             department.AreaCoordinates.Add(new DepartmentAreaCoordinate
        //             {
        //                 Department = department,
        //                 X = coordinate.X,
        //                 Y = coordinate.Y,
        //             });
        //         }
        //         else
        //         {
        //             var areaCoordinate = department.AreaCoordinates.FirstOrDefault(x => x.Id == coordinate.Id);
        //             if (areaCoordinate is null)
        //             {
        //                 continue;
        //             }
        //
        //             areaCoordinate.X = coordinate.X;
        //             areaCoordinate.Y = coordinate.Y;
        //         }
        //     }
        // }
        //
        // databaseContext.SaveChanges();

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.EDIT", "LABEL.DEPARTMENT_AREA_MANAGEMENT.SUCCESS.EDIT");
    }

    private class DepartmentUserCountDto
    {
        public int DepartmentId { get; set; }

        public int UserCount { get; set; }
    }
}