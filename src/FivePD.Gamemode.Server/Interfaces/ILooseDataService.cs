// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using FivePD.Gamemode.Server.Models.LooseDataSet;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Interfaces
{
    public interface ILooseDataService
    {
        public void RegisterSetWithAttributes<T>(List<LooseDataAttribute> attributes)
            where T : LooseDataSet;

        public void AddEntity<TS, TE>(TE entityToAdd)
            where TS : LooseDataSet
            where TE : LooseDataEntity;

        public T GetEntity<T>(int id);

        public void DeleteEntity<TS, TE>(int id)
            where TS : LooseDataSet
            where TE : LooseDataEntity;
    }
}