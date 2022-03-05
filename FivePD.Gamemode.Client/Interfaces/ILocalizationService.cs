// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Threading.Tasks;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Provides methods to request translations with their keys.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Register all localization related event handlers.
        /// </summary>
        public void RegisterEventHandlers();

        /// <summary>
        /// Finds and loads all translation files located in fivepd/Localization.
        /// </summary>
        public void LoadAvailableLocales();

        /// <summary>
        /// Loads the default localization file.
        /// </summary>
        public void LoadDefaultLocalization();

        /// <summary>
        /// Loads the passed in localization.
        /// </summary>
        /// <param name="locale">The localization's key.</param>
        public void LoadLocalization(string locale);
    }
}