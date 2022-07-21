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
using FivePD.Gamemode.Server.Database;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models;
using FivePD.Gamemode.Server.NuiModels;
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

        /// <inheritdoc/>
        void IServiceWithEvents.RegisterEvents()
        {
            this._eventHandlerDictionary[Events.Department.List] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetDepartmentsForNui);
            this._eventHandlerDictionary[Events.Department.GetById] += new Action<int, NetworkCallbackDelegate>(this.GetDepartmentByIdForNui);
            this._eventHandlerDictionary[Events.Department.Create] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.CreateDepartment);
            this._eventHandlerDictionary[Events.Department.Join] += new Action<Player, int, NetworkCallbackDelegate>(this.JoinOrLeaveDepartment);
            this._eventHandlerDictionary[Events.Department.Leave] += new Action<Player, int, NetworkCallbackDelegate>(this.JoinOrLeaveDepartment);
        }

        /// <inheritdoc />
        public bool CreateDepartment(Department department)
        {
            try
            {
                this._databaseContext.Departments.Add(department);
                this._databaseContext.SaveChanges();
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

        private void GetDepartmentsForNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var departments = this._databaseContext.Departments.AsNoTracking().ToList();
                networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(departments));
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
                    .Include(department => department.Users)
                    .Include(department => department.Ranks)
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

        private void CreateDepartment(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            try
            {
                var departmentFields = JsonConvert.DeserializeObject<CreateDepartmentFields>(JsonConvert.SerializeObject(expandoObject))!;
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

                string departmentAcronym;
                if (string.IsNullOrEmpty(departmentFields.Acronym))
                {
                    departmentAcronym = string.Join(
                        string.Empty,
                        departmentFields.Name
                            .Split()
                            .Where(s => s.Length > 0 && char.IsLetter(s[0]))
                            .Select(s => s[0])
                            .ToArray());
                }
                else
                {
                    departmentAcronym = departmentFields.Acronym;
                }

                this._databaseContext.Departments.Add(new Department
                {
                    Name = departmentFields.Name,
                    Acronym = departmentAcronym,
                    CallsignFormat = departmentFields.CallsignFormat,
                    MaxUsers = departmentFields.MaxUsers,
                });
                this._databaseContext.SaveChanges();
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = true,
                    Message = "Nui.Labels.SuccessfulDepartmentCreate",
                }.ToString());
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

        private void JoinOrLeaveDepartment([FromSource] Player player, int departmentId, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var license = player.Identifiers["license"];
            if (string.IsNullOrEmpty(license))
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                }.ToString());
                return;
            }

            try
            {
                var user = this._databaseContext.Users.FirstOrDefault(user => user.License == license);
                if (user is null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                    }.ToString());
                }
                else
                {
                    user.DepartmentId = departmentId == 0 ? (int?)null : departmentId;
                    user.DivisionId = null;
                    user.RankId = null;
                    this._databaseContext.SaveChanges();
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = true,
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
    }
}