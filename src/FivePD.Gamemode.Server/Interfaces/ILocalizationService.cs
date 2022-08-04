// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

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
    }
}