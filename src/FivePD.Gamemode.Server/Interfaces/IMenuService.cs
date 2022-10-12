// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Gamemode.Server.UIModels;

#pragma warning disable CS1591
#pragma warning disable SA1600

#pragma warning disable S125
#pragma warning disable SA1515

namespace FivePD.Gamemode.Server.Interfaces
{
    public interface IMenuService
    {
        public void Register<T>()
            where T : Menu;
    }
}