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

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Services
{
    public class ReportService : IReportService, IServiceWithEvents
    {
        private readonly ILooseDataService _looseDataService;

        public ReportService(ILooseDataService looseDataService)
        {
            this._looseDataService = looseDataService;
        }

        public IDisposable OnArrestReportAdd(Action<ArrestReport> observer)
        {
            return this._looseDataService.OnAdd(observer);
        }

        public IDisposable OnArrestReportEdit(Action<ArrestReport> observer)
        {
            return this._looseDataService.OnEdit(observer);
        }

        public IDisposable OnArrestReportDelete(Action<ArrestReport> observer)
        {
            return this._looseDataService.OnDelete(observer);
        }

        public void AddArrestReport(ArrestReport arrestReport)
        {
            this._looseDataService.AddEntity<ArrestReportSet, ArrestReport>(arrestReport);
        }

        public void UpdateArrestReport(ArrestReport arrestReport)
        {
            this._looseDataService.UpdateEntity(arrestReport);
        }

        public void DeleteArrestReport(int id)
        {
            this._looseDataService.DeleteEntity<ArrestReport>(id);
        }

        public ArrestReport GetArrestReport(int id)
        {
            return this._looseDataService.GetEntity<ArrestReport>(id);
        }

        public List<ArrestReport> GetArrestReports()
        {
            return this._looseDataService.GetEntities<ArrestReportSet, ArrestReport>();
        }

        public void RegisterEvents()
        {
            this._looseDataService.RegisterSetAttributes<ArrestReportSet>(new List<LooseDataAttribute>
            {
                new LooseDataAttribute
                {
                    Name = "DateOfArrest",
                    Label = "Date of arrest",
                    DataType = LooseDataAttribute.LooseDataType.DateTime,
                    Required = true,
                },
                new LooseDataAttribute
                {
                    Name = "AddressOfArrest",
                    Label = "Address of arrest",
                    DataType = LooseDataAttribute.LooseDataType.String,
                    Required = true,
                },
                new LooseDataAttribute
                {
                    Name = "Weapons",
                    Label = "Weapons",
                    DataType = LooseDataAttribute.LooseDataType.String,
                    Required = false,
                },
                new LooseDataAttribute
                {
                    Name = "Narrative",
                    Label = "Narrative",
                    DataType = LooseDataAttribute.LooseDataType.String,
                    Required = true,
                },
                new LooseDataAttribute
                {
                    Name = "ArresteeName",
                    Label = "Name of arrestee",
                    DataType = LooseDataAttribute.LooseDataType.String,
                    Required = true,
                },
                new LooseDataAttribute
                {
                    Name = "ArresteeBirthdate",
                    Label = "Birthdate of arrestee",
                    DataType = LooseDataAttribute.LooseDataType.DateTime,
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
                    DataType = LooseDataAttribute.LooseDataType.String,
                    Required = true,
                },
            });

            this._looseDataService.AddEntity<ArrestReportSet, ArrestReport>(new ArrestReport
            {
                Narrative = "test 123",
                Weapons = "weapons",
            });
        }
    }
}