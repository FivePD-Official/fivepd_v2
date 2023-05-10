using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Extensions;
using FivePD.Gamemode.Server.Attributes;
using FivePD.Gamemode.Server.Database;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models;
using FivePD.Gamemode.Server.Models.Requests;
using FivePD.Gamemode.Server.Models.Responses;
using FivePD.Gamemode.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc />
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
        public event EventHandler<Department> DepartmentEdited;

        /// <inheritdoc />
        public event EventHandler<Department> DepartmentDeleted;

        /// <inheritdoc />
        public event EventHandler<PlayerDepartmentChangeEventArgs> PlayerJoinedDepartment;

        /// <inheritdoc />
        public event EventHandler<PlayerDepartmentChangeEventArgs> PlayerLeftDepartment;

        /// <inheritdoc />
        public event EventHandler<Rank> PlayerRankCreated;

        /// <inheritdoc />
        public event EventHandler<Rank> PlayerRankEdited;

        /// <inheritdoc />
        public event EventHandler<Rank> PlayerRankDeleted;

        /// <inheritdoc />
        public event EventHandler<Division> PlayerDivisionCreated;

        /// <inheritdoc />
        public event EventHandler<Division> PlayerDivisionEdited;

        /// <inheritdoc />
        public event EventHandler<Division> PlayerDivisionDeleted;

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
            if (departmentExists != null)
            {
                throw new DepartmentExistsWithNameException();
            }

            try
            {
                if (string.IsNullOrEmpty(department.Callsign))
                {
                    department.Callsign = department.Name.ToAcronym();
                }

                var createdDepartmentEntry = this._databaseContext.Departments.Add(department);
                this._databaseContext.SaveChanges();
                this.DepartmentCreated?.Invoke(this, createdDepartmentEntry.Entity);
                return createdDepartmentEntry.Entity;
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        /// <inheritdoc />
        public void RemoveDepartment(Department department)
        {
            try
            {
                this._databaseContext.Departments.Remove(department);
                this._databaseContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        /// <inheritdoc />
        public IEnumerable<Department> GetDepartments(bool includeRanks = false, bool includeDivisions = false, bool includeUsers = false)
        {
            try
            {
                return this._databaseContext.Departments
                    .IncludeIf(includeDivisions, dept => dept.Divisions)
                    .IncludeIf(includeRanks, dept => dept.Ranks)
                    .IncludeIf(includeUsers, dept => dept.Users);
            }
            catch (Exception)
            {
                return new List<Department>();
            }
        }

        /// <inheritdoc />
        public Department GetDepartmentById(int id, bool includeRanks = false, bool includeDivisions = false, bool includeUsers = false)
        {
            try
            {
                return this._databaseContext.Departments
                    .IncludeIf(includeDivisions, dept => dept.Divisions)
                    .IncludeIf(includeRanks, dept => dept.Ranks)
                    .IncludeIf(includeUsers, dept => dept.Users)
                    .FirstOrDefault(department => department.Id == id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public bool SaveChanges()
        {
            try
            {
                this._databaseContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
            this.PlayerJoinedDepartment?.Invoke(this, new PlayerDepartmentChangeEventArgs
            {
                User = user,
                Department = department,
            });
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

            try
            {
                var department = user.Department;
                user.DepartmentId = null;
                user.DivisionId = null;
                user.RankId = null;
                this._databaseContext.SaveChanges();
                this.PlayerLeftDepartment?.Invoke(this, new PlayerDepartmentChangeEventArgs
                {
                    User = user,
                    Department = department,
                });
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        /// <inheritdoc />
        public void RemovePlayerFromDepartment(Player player)
        {
            this.RemovePlayerFromDepartment(this._playerDataService.FindUserByLicense(player));
        }

        [UIEvent(Events.UI.Department.List)]
        private void GetDepartmentsForNui(ExpandoObject departmentsRequestString, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
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

                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(page));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.GetById)]
        private void GetDepartmentByIdForNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var departmentRequest = expandoObject.ConvertTo<DepartmentByIdRequest>();
                var department = this._databaseContext.Departments
                    .AsNoTracking()
                    .Include(dept => dept.Divisions)
                    .Include(dept => dept.Ranks)
                    .FirstOrDefault(dept => dept.Id == departmentRequest.DepartmentId);
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(department));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.GetUsers)]
        private void GetDepartmentUsersForNui(ExpandoObject departmentUsersRequestString, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var departmentUsersRequest = departmentUsersRequestString.ConvertTo<DepartmentUsersRequest>();

                var filters = new Filters<User>();
                filters.Add(true, user => user.Department.Id == departmentUsersRequest.DepartmentId);
                filters.Add(
                    !string.IsNullOrEmpty(departmentUsersRequest.Filters.Name),
                    department => department.Name.ToLower().Contains(departmentUsersRequest.Filters.Name.ToLower()));

                var page = this._databaseContext.Users.AsNoTracking().Paginate(departmentUsersRequest.Page, filters);
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(page));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.GetRank)]
        private void GetDepartmentRankForNui(ExpandoObject requestString, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var request = requestString.ConvertTo<DepartmentRankRequest>();
                var rank = this._databaseContext.Ranks
                    .AsNoTracking()
                    .FirstOrDefault(rank => rank.Id == request.RankId);
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(rank));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.GetRanks)]
        private void GetDepartmentRanksForNui(ExpandoObject departmentRanksRequestString, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var departmentRanksRequest = departmentRanksRequestString.ConvertTo<DepartmentRanksRequest>();

                var filters = new Filters<Rank>();
                filters.Add(true, rank => rank.Department.Id == departmentRanksRequest.DepartmentId);
                filters.Add(
                    !string.IsNullOrEmpty(departmentRanksRequest.Filters.Name),
                    department => department.Name.ToLower().Contains(departmentRanksRequest.Filters.Name.ToLower()));

                var page = this._databaseContext.Ranks.AsNoTracking().Paginate(departmentRanksRequest.Page, filters);
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(page));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.GetDivision)]
        private void GetDepartmentDivisionForNui(ExpandoObject requestString, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var request = requestString.ConvertTo<DepartmentDivisionRequest>();

                var division = this._databaseContext.Divisions
                    .AsNoTracking()
                    .FirstOrDefault(division => division.Id == request.DivisionId);
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(division));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.GetDivisions)]
        private void GetDepartmentDivisionsForNui(ExpandoObject departmentDivisionsRequestString, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
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
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(page));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
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
            catch (Exception e)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.DeleteDivision)]
        private void DeleteDivisionFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var divisionFields = expandoObject.ConvertTo<DeleteDivisionRequest>();
                var divisionToDelete = this._databaseContext.Divisions
                    .Include(division => division.Users)
                    .Include(division => division.Department)
                    .FirstOrDefault(division => division.Id == divisionFields.DivisionId);
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
                this.PlayerDivisionDeleted?.Invoke(this, divisionToDelete);
                networkCallbackDelegate.InvokeWithResponse(
                    true,
                    "UI.LABEL.SUCCESS.DELETE",
                    "UI.LABEL.DIVISION.SUCCESS.DELETE",
                    new Dictionary<string, string>
                    {
                        { "division", divisionToDeleteName },
                    });
            }
            catch (Exception e)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.EditDivision)]
        private void EditDivisionFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
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
                this.PlayerDivisionEdited?.Invoke(this, divisionToEdit);
                networkCallbackDelegate.InvokeWithResponse(
                    true,
                    "UI.LABEL.SUCCESS.EDIT",
                    "UI.LABEL.DIVISION.SUCCESS.EDIT",
                    new Dictionary<string, string>
                    {
                        { "division", divisionToEditOldName },
                    });
            }
            catch (Exception e)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.CreateDivision)]
        private void CreateDivisionFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
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
                this.PlayerDivisionCreated?.Invoke(this, createdDivisionEntry.Entity);
                networkCallbackDelegate.InvokeWithResponse(
                    true,
                    "UI.LABEL.SUCCESS.CREATE",
                    "UI.LABEL.DIVISION.SUCCESS.CREATE",
                    new Dictionary<string, string>
                    {
                        { "division", createDivisionRequest.Name },
                    });
            }
            catch (Exception e)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.DeleteRank)]
        private void DeleteRankFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var deleteRankRequest = expandoObject.ConvertTo<DeleteRankRequest>();
                var rankToDelete = this._databaseContext.Ranks
                    .Include(rank => rank.Users)
                    .Include(rank => rank.Department)
                    .FirstOrDefault(rank => rank.Id == deleteRankRequest.RankId);
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
                this.PlayerRankDeleted?.Invoke(this, rankToDelete);
                networkCallbackDelegate.InvokeWithResponse(
                    true,
                    "UI.LABEL.SUCCESS.DELETE",
                    "UI.LABEL.RANK.SUCCESS.DELETE",
                    new Dictionary<string, string>
                    {
                        { "rank", rankToDeleteName },
                    });
            }
            catch (Exception e)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.EditRank)]
        private void EditRankFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
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
                this._databaseContext.SaveChanges();
                this.PlayerRankEdited?.Invoke(this, rankToEdit);
                networkCallbackDelegate.InvokeWithResponse(
                    true,
                    "UI.LABEL.SUCCESS.EDIT",
                    "UI.LABEL.RANK.SUCCESS.EDIT",
                    new Dictionary<string, string>
                    {
                        { "rank", rankToEditOldName },
                    });
            }
            catch (Exception e)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.CreateRank)]
        private void CreateRankFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
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
                });
                this._databaseContext.SaveChanges();
                this.PlayerRankCreated?.Invoke(this, createdRankEntry.Entity);
                networkCallbackDelegate.InvokeWithResponse(
                    true,
                    "UI.LABEL.SUCCESS.CREATE",
                    "UI.LABEL.RANK.SUCCESS.CREATE",
                    new Dictionary<string, string>
                    {
                        { "rank", createRankRequest.Name },
                    });
            }
            catch (Exception e)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.Edit)]
        private void EditDepartmentFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
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
                this.DepartmentEdited?.Invoke(this, department);
                networkCallbackDelegate.InvokeWithResponse(
                    true,
                    "UI.LABEL.SUCCESS.EDIT",
                    "UI.LABEL.DEPARTMENT.SUCCESS.EDIT",
                    new Dictionary<string, string>
                    {
                        { "department", departmentOldName },
                    });
            }
            catch (Exception e)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.CheckIfDepartmentNameTaken)]
        private void CheckIfDepartmentNameTaken(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
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

                networkCallbackDelegate.Invoke(new NameTakenResponse(departmentExists != null).ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new NameTakenResponse(false).ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.Delete)]
        private void DeleteDepartmentFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var deleteDepartmentRequest = expandoObject.ConvertTo<DeleteDepartmentRequest>();
                var department = this._databaseContext.Departments
                    .Include(department => department.Users)
                    .FirstOrDefault(department => department.Id == deleteDepartmentRequest.DepartmentId);
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
            catch (Exception e)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.Join, true)]
        private void PlayerJoinDepartmentFromNui([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var request = expandoObject.ConvertTo<JoinOrLeaveDepartmentRequest>();
            var department = this._databaseContext.Departments
                .Include(department => department.Users)
                .FirstOrDefault(department => department.Id == request.DepartmentId);

            try
            {
                this.AddPlayerToDepartment(player, department);
                if (department?.DefaultRankId != null)
                {
                    var user = this._playerDataService.FindUserByLicense(player);
                    user.RankId = department.DefaultRankId;
                    this._databaseContext.SaveChanges();
                }

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
            catch (Exception e)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        [UIEvent(Events.UI.Department.Leave, true)]
        private void PlayerLeaveDepartmentFromNui([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var playerDepartment = this._playerDataService.FindUserByLicense(player).Department;

            try
            {
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
            catch (Exception)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
            }
        }
    }
}