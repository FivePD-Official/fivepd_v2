// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Extensions;
using FivePD.Gamemode.Server.Database;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models;
using FivePD.Gamemode.Server.Models.Requests;
using FivePD.Gamemode.Server.Models.Responses;
using FivePD.Gamemode.Server.NuiModels;
using FivePD.Gamemode.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.IDepartmentService" />
    public class DepartmentService : IDepartmentService, IServiceWithEvents
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly DatabaseContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepartmentService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
        /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
        public DepartmentService(EventHandlerDictionary eventHandlerDictionary, DatabaseContext databaseContext)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._databaseContext = databaseContext;
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

        /// <inheritdoc/>
        public void RegisterEvents()
        {
            this._eventHandlerDictionary[Events.Department.List] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetDepartmentsForNui);
            this._eventHandlerDictionary[Events.Department.GetById] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetDepartmentByIdForNui);
            this._eventHandlerDictionary[Events.Department.GetUsers] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetDepartmentUsersForNui);
            this._eventHandlerDictionary[Events.Department.GetRanks] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetDepartmentRanksForNui);
            this._eventHandlerDictionary[Events.Department.GetDivisions] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetDepartmentDivisionsForNui);

            this._eventHandlerDictionary[Events.Department.Create] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.CreateDepartmentFromNui);
            this._eventHandlerDictionary[Events.Department.Join] += new Action<Player, int, NetworkCallbackDelegate>(this.PlayerJoinDepartmentFromNui);
            this._eventHandlerDictionary[Events.Department.Leave] += new Action<Player, int, NetworkCallbackDelegate>(this.PlayerLeaveDepartmentFromNui);
            this._eventHandlerDictionary[Events.Department.Edit] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.EditDepartmentFromNui);
            this._eventHandlerDictionary[Events.Department.Delete] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.DeleteDepartmentFromNui);
            this._eventHandlerDictionary[Events.Department.CheckIfDepartmentNameTaken] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.CheckIfDepartmentNameTaken);

            this._eventHandlerDictionary[Events.Department.DeleteDivision] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.DeleteDivisionFromNui);
            this._eventHandlerDictionary[Events.Department.EditDivision] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.EditDivisionFromNui);
            this._eventHandlerDictionary[Events.Department.CreateDivision] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.CreateDivisionFromNui);

            this._eventHandlerDictionary[Events.Department.DeleteRank] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.DeleteRankFromNui);
            this._eventHandlerDictionary[Events.Department.EditRank] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.EditRankFromNui);
            this._eventHandlerDictionary[Events.Department.CreateRank] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.CreateRankFromNui);

            var d = this.GetDepartmentById(5, false, true);
            if (d != null)
            {
                Debug.WriteLine($"{d.Ranks is null} - {d.Ranks?.Count()}");
                Debug.WriteLine($"{d.Divisions is null} - {d.Divisions?.Count()}");
            }
        }

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
            this.AddPlayerToDepartment(this.FindUserByLicense(player), department);
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
            this.RemovePlayerFromDepartment(this.FindUserByLicense(player));
        }

        private void GetDepartmentsForNui(ExpandoObject departmentsRequestString, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var departmentsRequest = departmentsRequestString.ConvertTo<DepartmentsRequest>();

                var filters = new Filters<Department>();
                filters.Add(!string.IsNullOrEmpty(departmentsRequest.Filters.Name), department => department.Name.ToLower().Contains(departmentsRequest.Filters.Name.ToLower()));
                filters.Add(!string.IsNullOrEmpty(departmentsRequest.Filters.Callsign), department => department.Callsign.ToLower().Contains(departmentsRequest.Filters.Callsign.ToLower()));

                var page = this._databaseContext.Departments
                    .AsNoTracking()
                    .Include(department => department.Users)
                    .Paginate(departmentsRequest.Page, filters);
                page.Items = page.Items.Select(item => item.ToResponse()).ToList();

                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(page));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

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

        private void GetDepartmentUsersForNui(ExpandoObject departmentUsersRequestString, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var departmentUsersRequest = departmentUsersRequestString.ConvertTo<DepartmentUsersRequest>();

                var filters = new Filters<User>();
                filters.Add(true, user => user.Department.Id == departmentUsersRequest.DepartmentId);
                filters.Add(!string.IsNullOrEmpty(departmentUsersRequest.Filters.Name), department => department.Name.ToLower().Contains(departmentUsersRequest.Filters.Name.ToLower()));

                var page = this._databaseContext.Users.AsNoTracking().Paginate(departmentUsersRequest.Page, filters);
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(page));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void GetDepartmentRanksForNui(ExpandoObject departmentRanksRequestString, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var departmentRanksRequest = departmentRanksRequestString.ConvertTo<DepartmentRanksRequest>();

                var filters = new Filters<Rank>();
                filters.Add(true, rank => rank.Department.Id == departmentRanksRequest.DepartmentId);
                filters.Add(!string.IsNullOrEmpty(departmentRanksRequest.Filters.Name), department => department.Name.ToLower().Contains(departmentRanksRequest.Filters.Name.ToLower()));

                var page = this._databaseContext.Ranks.AsNoTracking().Paginate(departmentRanksRequest.Page, filters);
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(page));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void GetDepartmentDivisionsForNui(ExpandoObject departmentDivisionsRequestString, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var departmentDivisionsRequest = departmentDivisionsRequestString.ConvertTo<DepartmentDivisionsRequest>();

                var filters = new Filters<Division>();
                filters.Add(true, division => division.Department.Id == departmentDivisionsRequest.DepartmentId);
                filters.Add(!string.IsNullOrEmpty(departmentDivisionsRequest.Filters.Name), department => department.Name.ToLower().Contains(departmentDivisionsRequest.Filters.Name.ToLower()));

                var page = this._databaseContext.Divisions.AsNoTracking().Paginate(departmentDivisionsRequest.Page, filters);
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(page));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

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
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Title = "Nui.Labels.SuccessfulCreate",
                    Description = "Nui.Labels.SuccessfulDepartmentCreate",
                    Params = new Dictionary<string, string>
                    {
                        { "department", createDepartmentRequest.Name },
                    },
                }.ToString());
            }
            catch (DepartmentExistsWithNameException)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.CreateDepartmentExistsWithNameTitle",
                    Description = "Nui.Errors.DepartmentExistsWithNameDescription",
                }.ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void DeleteDivisionFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var divisionFields = expandoObject.ConvertTo<DeleteDivisionRequest>();
                var divisionToDelete = this._databaseContext.Divisions
                    .Include(division => division.Users)
                    .FirstOrDefault(division => division.Id == divisionFields.DivisionId);
                if (divisionToDelete is null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.UnexpectedError",
                        Description = "Nui.Errors.DatabaseAccess",
                    }.ToString());
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
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Title = "Nui.Labels.SuccessfulDelete",
                    Description = "Nui.Labels.SuccessfulDivisionDelete",
                    Params = new Dictionary<string, string>
                    {
                        { "division", divisionToDeleteName },
                    },
                }.ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void EditDivisionFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var renameDivisionRequest = expandoObject.ConvertTo<RenameDivisionRequest>();
                var divisionToEdit = this._databaseContext.Divisions.FirstOrDefault(division => division.Id == renameDivisionRequest.DivisionId);
                if (divisionToEdit is null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.UnexpectedError",
                        Description = "Nui.Errors.DatabaseAccess",
                    }.ToString());
                    return;
                }

                var divisionExistsWithName = this._databaseContext.Divisions.FirstOrDefault(division => division.Name == renameDivisionRequest.Name && division.Department.Id == renameDivisionRequest.DepartmentId && division.Id != divisionToEdit.Id);
                if (divisionExistsWithName != null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.EditDivisionExistsWithNameTitle",
                        Description = "Nui.Errors.DivisionExistsWithNameDescription",
                    }.ToString());
                    return;
                }

                var divisionToEditOldName = divisionToEdit.Name;
                divisionToEdit.Name = renameDivisionRequest.Name;
                this._databaseContext.SaveChanges();
                this.PlayerDivisionEdited?.Invoke(this, divisionToEdit);
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Title = "Nui.Labels.SuccessfulEdit",
                    Description = "Nui.Labels.SuccessfulDivisionEdit",
                    Params = new Dictionary<string, string>
                    {
                        { "division", divisionToEditOldName },
                    },
                }.ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void CreateDivisionFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var createDivisionRequest = expandoObject.ConvertTo<CreateDivisionRequest>();
                var divisionExistsWithName = this._databaseContext.Divisions.FirstOrDefault(division => division.Name == createDivisionRequest.Name && division.Department.Id == createDivisionRequest.DepartmentId);
                if (divisionExistsWithName != null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.CreateDivisionExistsWithNameTitle",
                        Description = "Nui.Errors.DivisionExistsWithNameDescription",
                    }.ToString());
                    return;
                }

                var department = this._databaseContext.Departments.FirstOrDefault(department => department.Id == createDivisionRequest.DepartmentId);
                if (department is null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.UnexpectedError",
                        Description = "Nui.Errors.DatabaseAccess",
                    }.ToString());
                }

                var createdDivisionEntry = this._databaseContext.Divisions.Add(new Division
                {
                    Department = department,
                    Name = createDivisionRequest.Name,
                });
                this._databaseContext.SaveChanges();
                this.PlayerDivisionCreated?.Invoke(this, createdDivisionEntry.Entity);
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Title = "Nui.Labels.SuccessfulCreate",
                    Description = "Nui.Labels.SuccessfulDivisionCreate",
                    Params = new Dictionary<string, string>
                    {
                        { "division", createDivisionRequest.Name },
                    },
                }.ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

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
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.UnexpectedError",
                        Description = "Nui.Errors.DatabaseAccess",
                    }.ToString());
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
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Title = "Nui.Labels.SuccessfulDelete",
                    Description = "Nui.Labels.SuccessfulRankDelete",
                    Params = new Dictionary<string, string>
                    {
                        { "rank", rankToDeleteName },
                    },
                }.ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void EditRankFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var editRankRequest = expandoObject.ConvertTo<EditRankRequest>();
                var rankToEdit = this._databaseContext.Ranks.FirstOrDefault(rank => rank.Id == editRankRequest.RankId);
                if (rankToEdit is null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.UnexpectedError",
                        Description = "Nui.Errors.DatabaseAccess",
                    }.ToString());
                    return;
                }

                var rankExistsWithName = this._databaseContext.Ranks.FirstOrDefault(rank => rank.Name == editRankRequest.Name && rank.Department.Id == editRankRequest.DepartmentId && rank.Id != rankToEdit.Id);
                if (rankExistsWithName != null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.EditRankExistsWithNameTitle",
                        Description = "Nui.Errors.RankExistsWithNameDescription",
                    }.ToString());
                    return;
                }

                var rankToEditOldName = rankToEdit.Name;
                rankToEdit.Name = editRankRequest.Name;
                rankToEdit.Permission = editRankRequest.Permission;
                this._databaseContext.SaveChanges();
                this.PlayerRankEdited?.Invoke(this, rankToEdit);
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Title = "Nui.Labels.SuccessfulEdit",
                    Description = "Nui.Labels.SuccessfulRankEdit",
                    Params = new Dictionary<string, string>
                    {
                        { "rank", rankToEditOldName },
                    },
                }.ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void CreateRankFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var createRankRequest = expandoObject.ConvertTo<CreateRankRequest>();
                var rankExistsWithName = this._databaseContext.Ranks.FirstOrDefault(rank => rank.Name == createRankRequest.Name && rank.Department.Id == createRankRequest.DepartmentId);
                if (rankExistsWithName != null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.CreateRankExistsWithNameTitle",
                        Description = "Nui.Errors.RankExistsWithNameDescription",
                    }.ToString());
                    return;
                }

                var department = this._databaseContext.Departments.FirstOrDefault(department => department.Id == createRankRequest.DepartmentId);
                if (department is null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.UnexpectedError",
                        Description = "Nui.Errors.DatabaseAccess",
                    }.ToString());
                }

                var createdRankEntry = this._databaseContext.Ranks.Add(new Rank
                {
                    Department = department,
                    Name = createRankRequest.Name,
                    Permission = createRankRequest.Permission,
                });
                this._databaseContext.SaveChanges();
                this.PlayerRankCreated?.Invoke(this, createdRankEntry.Entity);
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Title = "Nui.Labels.SuccessfulCreate",
                    Description = "Nui.Labels.SuccessfulRankCreate",
                    Params = new Dictionary<string, string>
                    {
                        { "rank", createRankRequest.Name },
                    },
                }.ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void EditDepartmentFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var editDepartmentRequest = expandoObject.ConvertTo<EditDepartmentRequest>();
                var department = this._databaseContext.Departments.FirstOrDefault(department => department.Id == editDepartmentRequest.DepartmentId);
                if (department is null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.UnexpectedError",
                        Description = "Nui.Errors.DatabaseAccess",
                    }.ToString());
                    return;
                }

                var departmentExistsWithName = this._databaseContext.Departments.AsNoTracking().FirstOrDefault(dept => dept.Id != editDepartmentRequest.DepartmentId && dept.Name == editDepartmentRequest.Name);
                if (departmentExistsWithName != null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.EditDepartmentExistsWithNameTitle",
                        Description = "Nui.Errors.DepartmentExistsWithNameDescription",
                    }.ToString());
                    return;
                }

                if (editDepartmentRequest.DefaultRankId != 0)
                {
                    var defaultRank = this._databaseContext.Ranks.FirstOrDefault(rank => rank.Id == editDepartmentRequest.DefaultRankId);
                    if (defaultRank is null)
                    {
                        networkCallbackDelegate.Invoke(new ActionResponse
                        {
                            Status = false,
                            Title = "Nui.Errors.UnexpectedError",
                            Description = "Nui.Errors.DatabaseAccess",
                        }.ToString());
                        return;
                    }
                }

                var departmentOldName = department.Name;
                department.Name = editDepartmentRequest.Name;
                department.Callsign = string.IsNullOrEmpty(editDepartmentRequest.Callsign) ? editDepartmentRequest.Name.ToAcronym() : editDepartmentRequest.Callsign;
                department.CallsignFormat = editDepartmentRequest.CallsignFormat;
                department.MaxUsers = editDepartmentRequest.MaxUsers;
                department.DefaultRankId = editDepartmentRequest.DefaultRankId == 0 ? (int?)null : editDepartmentRequest.DefaultRankId;
                this._databaseContext.SaveChanges();
                this.DepartmentEdited?.Invoke(this, department);
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Title = "Nui.Labels.SuccessfulEdit",
                    Description = "Nui.Labels.SuccessfulDepartmentEdit",
                    Params = new Dictionary<string, string>
                    {
                        { "department", departmentOldName },
                    },
                }.ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void CheckIfDepartmentNameTaken(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var request = expandoObject.ConvertTo<NameTakenRequest>();
                var departmentExists = this._databaseContext.Departments.FirstOrDefault(department => department.Name == request.Name); // TODO: make it case sensitive?
                networkCallbackDelegate.Invoke(new NameTakenResponse(departmentExists != null).ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new NameTakenResponse(false).ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

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
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "Nui.Errors.UnexpectedError",
                        Description = "Nui.Errors.DatabaseAccess",
                    }.ToString());
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
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Title = "Nui.Labels.SuccessfulDelete",
                    Description = "Nui.Labels.SuccessfulDepartmentDelete",
                    Params = new Dictionary<string, string>
                    {
                        { "department", departmentOldName },
                    },
                }.ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void PlayerJoinDepartmentFromNui([FromSource] Player player, int departmentId, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var department = this._databaseContext.Departments
                .Include(department => department.Users)
                .FirstOrDefault(department => department.Id == departmentId);

            try
            {
                this.AddPlayerToDepartment(player, department);
                if (department?.DefaultRankId != null)
                {
                    var user = this.FindUserByLicense(player);
                    user.RankId = department.DefaultRankId;
                    this._databaseContext.SaveChanges();
                }

                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Title = "Nui.Labels.SuccessfulJoin",
                    Description = "Nui.Labels.SuccessfulDepartmentJoin",
                    Params = new Dictionary<string, string>
                    {
                        { "department", department!.Name },
                    },
                }.ToString());
            }
            catch (DepartmentFullException)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.DepartmentFullTitle",
                    Description = "Nui.Errors.DepartmentFullDescription",
                }.ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void PlayerLeaveDepartmentFromNui([FromSource] Player player, int departmentId, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var playerDepartment = this.FindUserByLicense(player).Department;

            try
            {
                this.RemovePlayerFromDepartment(player);
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Title = "Nui.Labels.SuccessfulLeave",
                    Description = "Nui.Labels.SuccessfulDepartmentLeave",
                    Params = new Dictionary<string, string>
                    {
                        { "department", playerDepartment!.Name },
                    },
                }.ToString());
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private User FindUserByLicense(Player player)
        {
            var license = player.Identifiers["license"];
            if (string.IsNullOrEmpty(license))
            {
                return null;
            }

            try
            {
                return this._databaseContext.Users.FirstOrDefault(user => user.License == license);
            }
            catch (Exception e)
            {
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }
    }
}