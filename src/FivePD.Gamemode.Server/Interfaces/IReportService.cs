// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using FivePD.Gamemode.Server.Models;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Interfaces
{
    public interface IReportService
    {
        public void AddArrestReport(ArrestReport arrestReport);

        public void UpdateArrestReport(ArrestReport arrestReport);

        public void DeleteArrestReport(int id);

        public ArrestReport GetById(int id);

        public List<ArrestReport> GetArrestReports();
    }
}