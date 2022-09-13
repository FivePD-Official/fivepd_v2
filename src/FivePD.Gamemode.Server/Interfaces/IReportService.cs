// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using FivePD.Gamemode.Server.Models;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Interfaces
{
    public interface IReportService
    {
        public IDisposable OnArrestReportAdd(Action<ArrestReport> observer);

        public IDisposable OnArrestReportEdit(Action<ArrestReport> observer);

        public IDisposable OnArrestReportDelete(Action<ArrestReport> observer);

        public void AddArrestReport(ArrestReport arrestReport);

        public void UpdateArrestReport(ArrestReport arrestReport);

        public void DeleteArrestReport(int id);

        public ArrestReport GetArrestReport(int id);

        public List<ArrestReport> GetArrestReports();
    }
}