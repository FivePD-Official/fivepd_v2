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
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Events;
using FivePD.Server.Models.Requests;
using FivePD.Server.Repositories;
using FivePD.Server.Utilities;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class DepartmentService : IDepartmentService
{
    private readonly DepartmentRepository _departmentRepository;
    private readonly UserRepository _userRepository;
    private readonly DivisionRepository _divisionRepository;
    private readonly RankRepository _rankRepository;
    private readonly DepartmentAreaCoordinateRepository _departmentAreaCoordinateRepository;
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
    /// <param name="departmentAreaCoordinateRepository">The <see cref="DepartmentAreaCoordinateRepository"/> to use.</param>
    /// <param name="playerDataService">The <see cref="PlayerDataService"/> to use.</param>
    /// <param name="playerHandlerService">The <see cref="PlayerHandlerService"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    /// <param name="dbConnectionFactory">The <see cref="IDapperDatabaseConnectionFactory"/> to use.</param>
    public DepartmentService(
        DepartmentRepository departmentRepository,
        UserRepository userRepository,
        DivisionRepository divisionRepository,
        RankRepository rankRepository,
        DepartmentAreaCoordinateRepository departmentAreaCoordinateRepository,
        IPlayerDataService playerDataService,
        IPlayerHandlerService playerHandlerService,
        INetworkEventService networkEventService,
        IDapperDatabaseConnectionFactory dbConnectionFactory)
    {
        this._departmentRepository = departmentRepository;
        this._userRepository = userRepository;
        this._divisionRepository = divisionRepository;
        this._rankRepository = rankRepository;
        this._departmentAreaCoordinateRepository = departmentAreaCoordinateRepository;
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
        var page = this._departmentRepository.Paginate(
            request.Page,
            b => b
                .If(!string.IsNullOrEmpty(request.Filters.Name), x => x.WhereContains("Name", request.Filters.Name))
                .If(!string.IsNullOrEmpty(request.Filters.Callsign), x => x.WhereContains("Callsign", request.Filters.Callsign))
                .If(request.Filters.UserDepartment, x => x.Where("Id = @Id", new { Id = user.DepartmentId })));
        if (!page.Items.Any())
        {
            this._networkEventService.TriggerEventSerialized(player, guid, page);
            return;
        }

        var builderResult = SqlBuilder
            .From($"{TableNames.Departments} d")
            .Select("d.Id as DepartmentId, COUNT(u.Id) as UserCount")
            .GroupBy("d.Id")
            .LeftJoin(TableNames.Users, "u", "d.Id = u.DepartmentId")
            .WhereIn("d.Id", page.Items.Select(d => d.Id))
            .Build();
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var departmentUsersCount = connection.Query<DepartmentUserCountDto>(builderResult.Sql, builderResult.Parameters).ToDictionary(c => c.DepartmentId);
        page.Items = page.Items
            .Select(item => departmentUsersCount.TryGetValue(item.Id, out var x) ? new DepartmentResponse(item, x.UserCount) : null)
            .ToList();
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.Department.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var department = this._departmentRepository.FindById(request.Id);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var divisionBuilderResult = SqlBuilder
            .From(TableNames.Divisions)
            .Where("DepartmentId = @DepartmentId", new { DepartmentId = department.Id })
            .Build();
        department.Divisions = connection.Query<Division>(divisionBuilderResult.Sql, divisionBuilderResult.Parameters).ToList();

        var rankBuilderResult = SqlBuilder
            .From($"{TableNames.DepartmentRanks} dr")
            .Select("dr.*, r.*")
            .InnerJoin(TableNames.Ranks, "r", $"dr.RankId = r.Id")
            .Where("dr.DepartmentId = @DepartmentId", new { DepartmentId = department.Id })
            .Build();
        department.DepartmentRanks = connection.Query<DepartmentRank, Rank, DepartmentRank>(
            rankBuilderResult.Sql,
            (deptRank, rank) =>
            {
                deptRank.Rank = rank;
                return deptRank;
            },
            rankBuilderResult.Parameters).ToList();

        this._networkEventService.TriggerEventSerialized(player, guid, department);
    }

    [UIEvent(Events.UI.Department.GetUsers)]
    private void GetDepartmentUsersForNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<DepartmentUsersRequest>(serializedRequest);
        var page = this._userRepository.Paginate(
            request.Page,
            b => b
                .If(true, x => x.Where("DepartmentId = @DepartmentId", new { DepartmentId = request.DepartmentId }))
                .If(!string.IsNullOrEmpty(request.Filters.Name), x => x.WhereContains("Name", request.Filters.Name)));
        if (!page.Items.Any())
        {
            this._networkEventService.TriggerEventSerialized(player, guid, page);
            return;
        }

        var builderResult = SqlBuilder
            .From(TableNames.UserCallsigns)
            .InnerJoin(TableNames.Users, "u", $"u.UserCallsignFormatId = {TableNames.UserCallsigns}.Id")
            .WhereIn("u.Id", page.Items.Select(d => d.Id))
            .Build();
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var callsigns = connection.Query<UserCallsignFormat>(builderResult.Sql, builderResult.Parameters).ToDictionary(c => c.Id);
        foreach (var user in page.Items)
        {
            if (user.UserCallsignFormatId.HasValue)
            {
                user.CallsignFormat = callsigns[user.UserCallsignFormatId.Value];
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
        var page = this._divisionRepository.Paginate(
            request.Page,
            b => b
                .If(true, x => x.Where("DepartmentId = @DepartmentId", new { DepartmentId = request.DepartmentId }))
                .If(!string.IsNullOrEmpty(request.Filters.Name), x => x.WhereContains("Name", request.Filters.Name)));
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.Department.Create)]
    private void CreateDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CreateDepartmentRequest>(serializedRequest);
        var builderResult = SqlBuilder
            .From(TableNames.Departments)
            .Where("Name = @Name", new { Name = request.Name })
            .BuildCount();
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var departmentWithSameName = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
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

        var department = this._departmentRepository.Add(new Department
        {
            Name = request.Name,
            Callsign = request.Callsign,
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
        this.DepartmentCreated?.Invoke(this, department);
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

        var builderResult = SqlBuilder
            .From(TableNames.Divisions)
            .Where("Name = @Name", new { Name = request.Name })
            .Where("DepartmentId = @DepartmentId", new { DepartmentId = request.DepartmentId })
            .BuildCount();
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var divisionWithSameName = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
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
        this.DivisionUpdated?.Invoke(this, division);
    }

    [UIEvent(Events.UI.Department.CreateDivision)]
    private void CreateDivisionFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CreateDivisionRequest>(serializedRequest);
        var builderResult = SqlBuilder
            .From(TableNames.Divisions)
            .Where("Name = @Name", new { Name = request.Name })
            .Where("DepartmentId = @DepartmentId", new { DepartmentId = request.DepartmentId })
            .BuildCount();
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var divisionWithSameName = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
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

        var division = this._divisionRepository.Add(new Division
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
        this.DivisionCreated?.Invoke(this, division);
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

        var builderResult = SqlBuilder
            .From(TableNames.Departments)
            .Where("Name = @Name", new { Name = request.Name })
            .Where("Id != @Id", new { Id = request.DepartmentId })
            .BuildCount();
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var departmentWithSameName = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
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

        var oldUseGlobalRanks = department.UseGlobalRanks;
        this._departmentRepository.Update(department, x =>
        {
            x.Name = request.Name;
            x.Callsign = string.IsNullOrEmpty(request.Callsign)
                ? request.Name.ToAcronym()
                : request.Callsign;
            x.MaxUsers = request.MaxUsers;
            x.DefaultRankId = request.DefaultRankId == -1 ? null : request.DefaultRankId;
            x.UseGlobalRanks = request.UseGlobalRanks;
        });

        if (oldUseGlobalRanks != request.UseGlobalRanks)
        {
            var users = this._departmentRepository.GetUsers(department);
            foreach (var user in users)
            {
                this._userRepository.Update(user, x => { x.RankId = null; });
            }
        }

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
        this.DepartmentUpdated?.Invoke(this, department);
    }

    [UIEvent(Events.UI.Department.EditCallsignFormat)]
    private void EditDepartmentCallsignFormatUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<EditDepartmentCallsignFormatRequest>(serializedRequest);
        var department = this._departmentRepository.FindById(request.DepartmentId);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._departmentRepository.Update(department, x => { x.CallsignFormat = request.CallsignFormat; });

        var departmentUsers = this._departmentRepository.GetUsers(department).ToList();
        foreach (var user in departmentUsers)
        {
            var callsignGenerator = new CallsignGenerator(department.CallsignFormat);
            string callsign;
            do
            {
                callsign = callsignGenerator.GenerateCallsign().BuildCallsign();
            }
            while (departmentUsers.Any(x => x.Callsign == callsign));

            user.Callsign = callsign;
            this._userRepository.Update(user, x => { x.Callsign = callsign; });

            var userCallsignFormat = this._userRepository.GetCallsignFormat(user);
            if (userCallsignFormat is not null)
            {
                this._userRepository.UpdateCallsignFormat(userCallsignFormat, x => { x.Values = callsignGenerator.GetGeneratedItemValues(); });
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
            "LABEL.SUCCESS.EDIT",
            "LABEL.DEPARTMENT.SUCCESS.EDIT",
            new Dictionary<string, string>
            {
                { "department", department.Name },
            });
    }

    [UIEvent(Events.UI.Department.CheckIfDepartmentNameTaken)]
    private void CheckIfDepartmentNameTaken([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<NameTakenRequest>(serializedRequest);
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var builderResult = SqlBuilder
            .From(TableNames.Departments)
            .Where("Name = @Name", new { Name = request.Name })
            .If(request.CheckForDepartmentId, b => b.Where("Id != @Id", new { Id = request.DepartmentId }))
            .BuildCount();
        var count = connection.ExecuteScalar<int>(builderResult.Sql, builderResult.Parameters);
        this._networkEventService.TriggerEventSerialized(player, guid, new NameTakenResponse(count > 0));
    }

    [UIEvent(Events.UI.Department.Delete)]
    private void DeleteDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var department = this._departmentRepository.FindById(request.Id);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._departmentRepository.Delete(request.Id);
        this.DepartmentDeleted?.Invoke(this, department);
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.DELETE",
            "LABEL.DEPARTMENT.SUCCESS.DELETE",
            new Dictionary<string, string>
            {
                { "department", department.Name },
            });
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

        var departmentUsersCount = this._departmentRepository.GetUsers(department).Count();
        if (department.MaxUsers != 0 && department.MaxUsers == departmentUsersCount)
        {
            this._networkEventService.TriggerEventWithResponse(player, guid, false, "ERROR.DEPARTMENT.FULL.TITLE", "ERROR.DEPARTMENT.FULL.DESCRIPTION");
            return;
        }

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

            var userCallsignFormat = this._userRepository.GetCallsignFormat(user);
            if (userCallsignFormat is not null)
            {
                this._userRepository.UpdateCallsignFormat(userCallsignFormat, x => { x.Values = callsignGenerator.GetGeneratedItemValues(); });
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
        this.PlayerJoinedDepartment?.Invoke(this, new PlayerDepartmentChangeEventArgs(user, department));
    }

    [UIEvent(Events.UI.Department.Leave)]
    private void PlayerLeaveDepartmentFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var user = this._playerDataService.FindUserByLicense(player);
        if (user is null || !user.DepartmentId.HasValue)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var department = this._departmentRepository.FindById(user.DepartmentId.Value);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        if (user.UserCallsignFormatId.HasValue)
        {
            this._userRepository.DeleteCallsignFormat(user, user.UserCallsignFormatId.Value);
        }

        this._userRepository.Update(user, x =>
        {
            x.DepartmentId = null;
            x.DivisionId = null;
            x.RankId = null;
            x.Callsign = string.Empty;
        });
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.LEAVE",
            "LABEL.DEPARTMENT.SUCCESS.LEAVE",
            new Dictionary<string, string>
            {
                { "department", department.Name },
            });
        this.PlayerLeftDepartment?.Invoke(this, new PlayerDepartmentChangeEventArgs(user, department));
    }

    [UIEvent(Events.UI.Department.RemoveUser)]
    private void KickPlayerFromDepartmentNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var idRequest = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var user = this._userRepository.FindById(idRequest.Id);
        if (user is null || !user.DepartmentId.HasValue)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var department = this._departmentRepository.FindById(user.DepartmentId.Value);
        if (department is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._userRepository.Update(user, x =>
        {
            x.DepartmentId = null;
            x.DivisionId = null;
            x.RankId = null;
        });
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "Kick", "Kicking player");
        this.PlayerLeftDepartment?.Invoke(this, new PlayerDepartmentChangeEventArgs(user, department));
    }

    [UIEvent(Events.UI.Department.GetAllSimpleWithArea)]
    private void GetAllSimpleWithAreaCoordinatesUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var departments = this._departmentRepository.GetAll().ToList();
        var builderResult = SqlBuilder.From(TableNames.DepartmentAreaCoordinates).Build();
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var departmentAreaCoordinates = connection.Query<DepartmentAreaCoordinate>(builderResult.Sql, builderResult.Parameters).ToList();
        var list = departments.Select(department => new SimpleDepartmentWithAreaDTO
        {
            Id = department.Id,
            Name = department.Name,
            Callsign = department.Callsign,
            Coordinates = departmentAreaCoordinates.FindAll(x => x.DepartmentId == department.Id),
        });
        this._networkEventService.TriggerEventSerialized(player, guid, list);
    }

    [UIEvent(Events.UI.Department.UpdateAreas)]
    private void UpdateAreasUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<UpdateDepartmentAreasRequest>(serializedRequest);
        foreach (var area in request.Areas)
        {
            if (!this._departmentRepository.Exists(area.DepartmentId))
            {
                this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
                return;
            }

            var idsToKeep = area.Coordinates.Select(x => x.Id).ToList();
            var builderResult = SqlBuilder
                .From(TableNames.DepartmentAreaCoordinates)
                .Delete()
                .Where("DepartmentId = @DepartmentId", new { DepartmentId = area.DepartmentId })
                .If(idsToKeep.Any(), b => b.WhereNotIn("Id", idsToKeep))
                .Build();
            using var connection = this._dbConnectionFactory.CreateScope().Connection;
            connection.Execute(builderResult.Sql, builderResult.Parameters);

            foreach (var coordinate in area.Coordinates)
            {
                if (coordinate.Id == -1)
                {
                    this._departmentAreaCoordinateRepository.Add(new DepartmentAreaCoordinate
                    {
                        DepartmentId = coordinate.DepartmentId,
                        X = coordinate.X,
                        Y = coordinate.Y,
                    });
                }
                else
                {
                    var areaCoordinate = this._departmentAreaCoordinateRepository.FindById(coordinate.Id);
                    if (areaCoordinate is null)
                    {
                        continue;
                    }

                    this._departmentAreaCoordinateRepository.Update(areaCoordinate, x =>
                    {
                        x.X = coordinate.X;
                        x.Y = coordinate.Y;
                    });
                }
            }
        }

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.EDIT", "LABEL.DEPARTMENT_AREA_MANAGEMENT.SUCCESS.EDIT");
    }

#pragma warning disable
    private class DepartmentUserCountDto
    {
        public int DepartmentId { get; set; }

        public int UserCount { get; set; }
    }
#pragma warning restore
}