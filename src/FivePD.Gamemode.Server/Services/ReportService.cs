// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models;
using FivePD.Gamemode.Server.Models.LooseDataSet;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.IReportService" />
    public class ReportService : IReportService, IServiceWithEvents
    {
        private readonly ILooseDataService _looseDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportService"/> class.
        /// </summary>
        /// <param name="looseDataService">The <see cref="ILooseDataService"/> to use.</param>
        public ReportService(ILooseDataService looseDataService)
        {
            this._looseDataService = looseDataService;
        }

        /// <inheritdoc />
        public IDisposable OnArrestReportAdd(Action<LooseDataEntity> observer)
        {
            return this._looseDataService.OnAdd<ArrestReportSet>(observer);
        }

        /// <inheritdoc />
        public IDisposable OnArrestReportEdit(Action<LooseDataEntity> observer)
        {
            return this._looseDataService.OnEdit<ArrestReportSet>(observer);
        }

        /// <inheritdoc />
        public IDisposable OnArrestReportDelete(Action<LooseDataEntity> observer)
        {
            return this._looseDataService.OnDelete<ArrestReportSet>(observer);
        }

        /// <inheritdoc />
        public void AddArrestReport(LooseDataEntity arrestReport)
        {
            this._looseDataService.AddEntity<ArrestReportSet>(arrestReport);
        }

        /// <inheritdoc />
        public void UpdateArrestReport(LooseDataEntity arrestReport)
        {
            this._looseDataService.UpdateEntity(arrestReport);
        }

        /// <inheritdoc />
        public void DeleteArrestReport(LooseDataEntity arrestReport)
        {
            this._looseDataService.DeleteEntity(arrestReport);
        }

        /// <inheritdoc />
        public LooseDataEntity GetArrestReport(int id)
        {
            return this._looseDataService.GetEntity(id);
        }

        /// <inheritdoc />
        public List<LooseDataEntity> GetArrestReports()
        {
            return this._looseDataService.GetEntities<ArrestReportSet>();
        }

        /// <inheritdoc />
        public void RegisterEvents()
        {
            this._looseDataService.RegisterSetAttributes<ArrestReportSet>(new List<LooseDataAttribute>
            {
                new LooseDataAttribute
                {
                    Name = "DepartmentId",
                    Label = "Department",
                    DataType = LooseDataAttribute.LooseDataType.Integer,
                    Required = false,
                    ShowInGrid = false,
                    Readonly = true,
                },
                new LooseDataAttribute
                {
                    Name = "DateOfSubmit",
                    Label = "Date of submit",
                    DataType = LooseDataAttribute.LooseDataType.Date,
                    Required = false,
                    ShowInGrid = true,
                    Readonly = true,
                },
                new LooseDataAttribute
                {
                    Name = "DateOfArrest",
                    Label = "Date of arrest",
                    DataType = LooseDataAttribute.LooseDataType.Date,
                    Required = true,
                    ShowInGrid = true,
                },
                new LooseDataAttribute
                {
                    Name = "AddressOfArrest",
                    Label = "Address of arrest",
                    DataType = LooseDataAttribute.LooseDataType.ShortString,
                    Required = true,
                },
                new LooseDataAttribute
                {
                    Name = "Weapons",
                    Label = "Weapons",
                    DataType = LooseDataAttribute.LooseDataType.ShortString,
                    Required = false,
                },
                new LooseDataAttribute
                {
                    Name = "Narrative",
                    Label = "Narrative",
                    DataType = LooseDataAttribute.LooseDataType.LongString,
                    Required = true,
                },
                new LooseDataAttribute
                {
                    Name = "ArresteeName",
                    Label = "Name of arrestee",
                    DataType = LooseDataAttribute.LooseDataType.ShortString,
                    Required = true,
                    ShowInGrid = true,
                    Order = 1,
                },
                new LooseDataAttribute
                {
                    Name = "ArresteeBirthdate",
                    Label = "Birthdate of arrestee",
                    DataType = LooseDataAttribute.LooseDataType.Date,
                    Required = true,
                },
                new LooseDataAttribute
                {
                    Name = "ArresteeAge",
                    Label = "Age of arrestee",
                    DataType = LooseDataAttribute.LooseDataType.Integer,
                    Required = true,
                },
                new LooseDataAttribute
                {
                    Name = "ArresteeGender",
                    Label = "Gender of arrestee",
                    DataType = LooseDataAttribute.LooseDataType.Integer,
                    Required = true,
                    Options = new LooseDataAttribute.AttributeOptions
                    {
                        Items = new Dictionary<int, string>
                        {
                            { (int)FGender.Male, "Male" },
                            { (int)FGender.Female, "Female" },
                        },
                    },
                },
                new LooseDataAttribute
                {
                    Name = "ArresteeRace",
                    Label = "Race of arrestee",
                    DataType = LooseDataAttribute.LooseDataType.ShortString,
                    Required = true,
                },
            });
        }
    }
}