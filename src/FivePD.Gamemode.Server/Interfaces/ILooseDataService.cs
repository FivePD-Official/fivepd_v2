// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using FivePD.Gamemode.Server.Models.LooseDataSet;

#pragma warning disable CS1591
#pragma warning disable SA1600

#pragma warning disable SA1649

namespace FivePD.Gamemode.Server.Interfaces
{
    public interface ILooseDataService
    {
        public void RegisterSetAttributes<TS>(List<LooseDataAttribute> attributes)
            where TS : LooseDataSet;

        public IDisposable OnAdd<TE>(Action<TE> observer)
            where TE : LooseDataEntity;

        public IDisposable OnEdit<TE>(Action<TE> observer)
            where TE : LooseDataEntity;

        public IDisposable OnDelete<TE>(Action<TE> observer)
            where TE : LooseDataEntity;

        public void AddEntity<TS, TE>(TE entityToAdd)
            where TS : LooseDataSet
            where TE : LooseDataEntity;

        public void UpdateEntity<TE>(TE entityToUpdate)
            where TE : LooseDataEntity;

        public void DeleteEntity<TE>(TE entityToDelete)
            where TE : LooseDataEntity;

        public TE GetEntity<TE>(int id)
            where TE : LooseDataEntity;

        public List<TE> GetEntities<TS, TE>()
            where TS : LooseDataSet
            where TE : LooseDataEntity;
    }
}