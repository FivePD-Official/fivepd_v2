// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models;
using FivePD.Gamemode.Server.Models.LooseDataSet;
using Newtonsoft.Json.Linq;

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

        public ArrestReport GetById(int id)
        {
            return this._looseDataService.GetEntity<ArrestReport>(id);
        }

        public void RegisterEvents()
        {
#pragma warning disable S125

            // var test = this.GetById(2);
            // Debug.WriteLine($"Id - {test.Id}");
            // Debug.WriteLine($"Narrative - {test.Narrative}");
            // Debug.WriteLine($"Weapons - {test.Weapons}");
            // Debug.WriteLine($"AddressOfArrest - {test.AddressOfArrest}");
            // Debug.WriteLine($"DateOfArrest - {test.DateOfArrest}");
            this._looseDataService.DeleteEntity<ArrestReportSet, ArrestReport>(5);

            // this._looseDataService.AddEntity<ArrestReportSet, ArrestReport>(new ArrestReport
            // {
            //     Narrative = "Narrative",
            //     Weapons = "Weapons",
            //     AddressOfArrest = "Addressofarrest",
            //     DateOfArrest = DateTime.Now,
            // });
            this._looseDataService.RegisterSetWithAttributes<ArrestReportSet>(new List<LooseDataAttribute>
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
                    Options = new JObject
                    {
                        { "items", new JArray(FGender.Male, FGender.Female) },
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
        }
    }
}