// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using FivePD.Gamemode.Server.Models;
using FivePD.Gamemode.Server.Models.LooseDataSet;

namespace FivePD.Gamemode.Server.Interfaces
{
    /// <summary>
    /// Provides easy access to reportable entities through <see cref="ILooseDataService"/>.
    /// </summary>
    public interface IReportService
    {
#pragma warning disable SA1600
#pragma warning disable CS1591
        public IDisposable OnArrestReportAdd(Action<LooseDataEntity> observer);

        public IDisposable OnArrestReportEdit(Action<LooseDataEntity> observer);

        public IDisposable OnArrestReportDelete(Action<LooseDataEntity> observer);

        public void AddArrestReport(LooseDataEntity arrestReport);

        public void UpdateArrestReport(LooseDataEntity arrestReport);

        public void DeleteArrestReport(LooseDataEntity arrestReport);

        public LooseDataEntity GetArrestReport(int id);

        public List<LooseDataEntity> GetArrestReports();
    }
}