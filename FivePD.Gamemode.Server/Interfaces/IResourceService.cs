// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using FivePD.Common.ConfigModels;
using FivePD.Common.Localization;

#pragma warning disable SA1618

namespace FivePD.Gamemode.Server.Interfaces
{
    /// <summary>
    /// Provides utility methods for the FivePD resource itself.
    /// </summary>
    public interface IResourceService
    {
        /// <summary>
        /// Loads and returns the /Localization/{locale}.json file's content.
        /// </summary>
        /// <param name="locale">The localization's key.</param>
        /// <returns>Returns a deserialized <see cref="Localization"/> object.</returns>
        public Localization GetLocalization(string locale);

        /// <summary>
        /// Loads and returns the /Config/items.json file's content.
        /// </summary>
        /// <returns>Returns all items from the config file.</returns>
        public List<Item> GetItems();
    }
}