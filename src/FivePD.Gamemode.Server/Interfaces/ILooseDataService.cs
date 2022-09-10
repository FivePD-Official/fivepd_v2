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

        public void UpdateEntity<T>(T entityToUpdate)
            where T : LooseDataEntity;

        public void DeleteEntity<T>(int id)
            where T : LooseDataSet;

        public T GetEntity<T>(int id)
            where T : LooseDataEntity;

        public List<TE> GetEntities<TS, TE>()
            where TS : LooseDataSet
            where TE : LooseDataEntity;
    }
}