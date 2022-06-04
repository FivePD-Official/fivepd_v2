// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using FivePD.Common;
using FivePD.Common.ConfigModels;
using FivePD.Common.Localization;

#pragma warning disable SA1618

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Provides utility methods for the FivePD resource itself.
    /// </summary>
    public interface IResourceService
    {
        /// <summary>
        /// Loads and returns the /Config/vehicles.json file's content.
        /// </summary>
        /// <returns>Returns a deserialized <see cref="Vehicles"/> object.</returns>
        public Vehicles GetVehiclesConfig();

        /// <summary>
        /// Loads and returns the /Config/menu.json file's content.
        /// </summary>
        /// <returns>Returns a deserialized a list of <see cref="Loadout"/> objects.</returns>
        public List<Loadout> GetLoadoutsConfig();

        /// <summary>
        /// Loads and returns the /Config/menu.json file's content.
        /// </summary>
        /// <returns>Returns a deserialized <see cref="MenuConfig"/> object.</returns>
        public MenuConfig GetMenuConfig();

        /// <summary>
        /// Loads and returns a /Localization/{locale}.json file.
        /// </summary>
        /// <param name="locale">The locale of the localization to load.</param>
        /// <returns>Returns a deserialized <see cref="Localization"/> object.</returns>
        public Localization GetLocalization(string locale);
    }
}