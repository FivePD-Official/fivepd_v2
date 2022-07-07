// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Server.Database;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc />
    public class DepartmentService : IDepartmentService
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
        public void RegisterEvents()
        {
            this._eventHandlerDictionary[Events.Department.List] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.GetDepartmentsForNui);
            this._eventHandlerDictionary[Events.Department.GetById] += new Action<int, NetworkCallbackDelegate>(this.GetDepartmentByIdForNui);
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
    }
}