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
using FivePD.Gamemode.Server.NuiModels;
using FivePD.Gamemode.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
        public event EventHandler<PlayerDepartmentChangeEventArgs> PlayerJoinedDepartment;

        /// <inheritdoc />
        public event EventHandler<PlayerDepartmentChangeEventArgs> PlayerLeftDepartment;

        /// <inheritdoc/>
        public void RegisterEvents()
        {
            this._eventHandlerDictionary[Events.Department.List] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetDepartmentsForNui);
            this._eventHandlerDictionary[Events.Department.GetById] += new Action<int, NetworkCallbackDelegate>(this.GetDepartmentByIdForNui);
            this._eventHandlerDictionary[Events.Department.GetUsers] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetDepartmentUsersForNui);
            this._eventHandlerDictionary[Events.Department.GetRanks] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetDepartmentRanksForNui);
            this._eventHandlerDictionary[Events.Department.Create] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.CreateDepartment);
            this._eventHandlerDictionary[Events.Department.Join] += new Action<Player, int, NetworkCallbackDelegate>(this.PlayerJoinDepartmentFromNui);
            this._eventHandlerDictionary[Events.Department.Leave] += new Action<Player, int, NetworkCallbackDelegate>(this.PlayerLeaveDepartmentFromNui);
        }

        /// <inheritdoc />
        public bool CreateDepartment(Department department)
        {
            if (department is null || string.IsNullOrEmpty(department.Name))
            {
                return false;
            }

            try
            {
                if (string.IsNullOrEmpty(department.Acronym))
                {
                    department.Acronym = department.Name.ToAcronym();
                }

                var createdDepartmentEntry = this._databaseContext.Departments.Add(department);
                this._databaseContext.SaveChanges();
                this.DepartmentCreated?.Invoke(this, createdDepartmentEntry.Entity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool RemoveDepartment(Department department)
        {
            try
            {
                this._databaseContext.Departments.Remove(department);
                this._databaseContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public IEnumerable<Department> GetDepartments()
        {
            try
            {
                return this._databaseContext.Departments;
            }
            catch (Exception)
            {
                return new List<Department>();
            }
        }

        /// <inheritdoc />
        public Department GetDepartmentById(int id)
        {
            try
            {
                return this._databaseContext.Departments.FirstOrDefault(department => department.Id == id);
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
        public bool AddPlayerToDepartment(User user, Department department)
        {
            if (user is null || department is null)
            {
                return false;
            }

            try
            {
                user.DepartmentId = department.Id;
                user.DivisionId = null;
                user.RankId = null;
                this._databaseContext.SaveChanges();
                this.PlayerJoinedDepartment?.Invoke(this, new PlayerDepartmentChangeEventArgs
                {
                    User = user,
                    Department = department,
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool AddPlayerToDepartment(Player player, Department department)
        {
            return this.AddPlayerToDepartment(this.FindUserByLicense(player), department);
        }

        /// <inheritdoc />
        public bool RemovePlayerFromDepartment(User user)
        {
            if (user is null)
            {
                return false;
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
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool RemovePlayerFromDepartment(Player player)
        {
            return this.RemovePlayerFromDepartment(this.FindUserByLicense(player));
        }

        private void GetDepartmentsForNui(ExpandoObject departmentsRequestString, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var departmentsRequest = departmentsRequestString.ConvertTo<DepartmentsRequest>();

                var filters = new Filters<Department>();
                filters.Add(!string.IsNullOrEmpty(departmentsRequest.Filters.Name), department => department.Name.ToLower().Contains(departmentsRequest.Filters.Name.ToLower()));
                filters.Add(!string.IsNullOrEmpty(departmentsRequest.Filters.Acronym), department => department.Acronym.ToLower().Contains(departmentsRequest.Filters.Acronym.ToLower()));

                var page = this._databaseContext.Departments.AsNoTracking().Paginate(departmentsRequest.Page, filters);
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(page));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void GetDepartmentByIdForNui(int id, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var department = this._databaseContext.Departments
                    .AsNoTracking()
                    .Include(department => department.Divisions)
                    .FirstOrDefault(department => department.Id == id);
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

        private void CreateDepartment(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var departmentFields = expandoObject.ConvertTo<CreateDepartmentFields>();
                var department = this._databaseContext.Departments.FirstOrDefault(department => department.Name == departmentFields.Name);
                if (department != null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Message = "Nui.Errors.DepartmentExistsWithName",
                    }.ToString());
                    return;
                }

                var createdDepartmentEntry = this._databaseContext.Departments.Add(new Department
                {
                    Name = departmentFields.Name,
                    Acronym = string.IsNullOrEmpty(departmentFields.Acronym) ? departmentFields.Name.ToAcronym() : departmentFields.Acronym,
                    CallsignFormat = departmentFields.CallsignFormat,
                    MaxUsers = departmentFields.MaxUsers,
                });
                this._databaseContext.SaveChanges();
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Message = "Nui.Labels.SuccessfulDepartmentCreate",
                }.ToString());
                this.DepartmentCreated?.Invoke(this, createdDepartmentEntry.Entity);
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Message = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void PlayerJoinDepartmentFromNui([FromSource] Player player, int departmentId, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var department = this._databaseContext.Departments.FirstOrDefault(department => department.Id == departmentId);
                if (department is null)
                {
                    return;
                }

                if (this.AddPlayerToDepartment(player, department))
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = true,
                        Message = "Nui.Labels.SuccessfulDepartmentJoin",
                    }.ToString());
                }
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Message = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

        private void PlayerLeaveDepartmentFromNui([FromSource] Player player, int departmentId, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                if (this.RemovePlayerFromDepartment(player))
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = true,
                        Message = "Nui.Labels.SuccessfulDepartmentLeave",
                    }.ToString());
                }
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Message = "Nui.Errors.DatabaseAccess",
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