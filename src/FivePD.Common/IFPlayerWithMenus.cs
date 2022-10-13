// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using FivePD.Common.Attributes;
using FivePD.Common.UIModels;

namespace FivePD.Common
{
    /// <summary>
    /// asd.
    /// </summary>
    [NoUsage]
    public interface IFPlayerWithMenus
    {
        /// <summary>
        /// qwe.
        /// </summary>
        /// <param name="menus">qweqwe.</param>
        void SetMenus(List<Menu> menus);

        /// <summary>
        /// qwe.
        /// </summary>
        /// <param name="hashcode">qweqwe2.</param>
        /// <returns>the.</returns>
        MenuItem FindMenuItem(string hashcode);
    }
}