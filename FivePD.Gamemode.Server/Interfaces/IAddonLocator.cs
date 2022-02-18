// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.Interfaces
{
    /**
     * <summary>Responsible for locating addon definitions. Loading of addons should be.</summary>
     */
    public interface IAddonLocator
    {
        /**
         * <summary>Returns all found addon definitions.</summary>
         * <returns>An enumerable of all addon definitions located.</returns>
         */
        public IEnumerable<AddonDefinition> GetAddons();
    }
}