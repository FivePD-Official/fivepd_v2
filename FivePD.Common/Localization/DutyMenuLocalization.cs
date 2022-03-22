// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using Newtonsoft.Json;

namespace FivePD.Common.Localization
{
    /// <summary>
    /// Contains all keys that are used within Duty menu.
    /// </summary>
    public class DutyMenuLocalization
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DutyMenuLocalization"/> class.
        /// </summary>
        public DutyMenuLocalization()
        {
            this.OnDuty = new OnDutyLocalization();
            this.GetLoadout = new GetLoadoutLocalization();
            this.ChangeLanguage = new ChangeLanguateLocalization();
        }

        /// <summary>Gets or sets the Title.</summary>
        public string Title { get; set; } = "Duty menu";

        /// <summary>
        /// Gets or sets the OnDutyItem.
        /// </summary>
        public OnDutyLocalization OnDuty { get; set; }

        /// <summary>
        /// Gets or sets the GetLoadoutItem.
        /// </summary>
        public GetLoadoutLocalization GetLoadout { get; set; }

        /// <summary>
        /// Gets or sets the ChangeLanguageItem.
        /// </summary>
        public ChangeLanguateLocalization ChangeLanguage { get; set; }

        /// <summary>
        /// Contains all keys that are used for the On duty item.
        /// </summary>
        public class OnDutyLocalization
        {
            /// <summary>
            /// Gets or sets the Title.
            /// </summary>
            public string Title { get; set; } = "On duty";

            /// <summary>
            /// Gets or sets the Description.
            /// </summary>
            public string Description { get; set; } = "Description of on duty.";
        }

        /// <summary>
        /// Contains all keys that are used for the Get loadout item.
        /// </summary>
        public class GetLoadoutLocalization
        {
            /// <summary>
            /// Gets or sets the Title.
            /// </summary>
            public string Title { get; set; } = "Get loadout";

            /// <summary>
            /// Gets or sets the Description.
            /// </summary>
            public string Description { get; set; } = "Description of get loadout";
        }

        /// <summary>
        /// Contains all keys that are used for the Change language item.
        /// </summary>
        public class ChangeLanguateLocalization
        {
            /// <summary>
            /// Gets or sets the Title.
            /// </summary>
            public string Title { get; set; } = "Change language";

            /// <summary>
            /// Gets or sets the Description.
            /// </summary>
            public string Description { get; set; } = "Description of change language";
        }
    }
}