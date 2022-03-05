// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

#pragma warning disable S1104
#pragma warning disable SA1401
namespace FivePD.Common.Schemas
{
    /// <summary>
    /// Contains all keys that are used within the whole project for translations.
    /// </summary>
    public class Localization
    {
        /// <summary>Gets title of the localization.</summary>
        public string Title;

        /// <summary>Gets localization of the duty menu.</summary>
        public DutyMenuLocalization DutyMenu;
    }
}
#pragma warning restore SA1401
#pragma warning restore S1104