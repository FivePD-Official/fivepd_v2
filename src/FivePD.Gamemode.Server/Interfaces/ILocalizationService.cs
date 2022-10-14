// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using FivePD.Common;
using FivePD.Common.Localization;

namespace FivePD.Gamemode.Server.Interfaces
{
    /// <summary>
    /// Provides utility methods to handle translation files.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets the default english localization.
        /// </summary>
        /// <returns>The default english localization.</returns>
        public Localization GetDefaultLocalization();

        /// <summary>
        /// Gets the available locales.
        /// </summary>
        /// <returns>The available locales.</returns>
        public List<Locale> GetLocales();

        /// <summary>
        /// Gets the localization with the given locale.
        /// </summary>
        /// <param name="locale">The locale of the localization.</param>
        /// <returns>The localization object.</returns>
        public Localization GetLocalization(string locale);

        /// <summary>
        /// Gets the localization at the given index.
        /// </summary>
        /// <param name="index">The index of the locale.</param>
        /// <returns>The localization object.</returns>
        public Localization GetLocalization(int index);
    }
}