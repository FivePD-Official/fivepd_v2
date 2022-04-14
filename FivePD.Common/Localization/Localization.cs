// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

namespace FivePD.Common.Localization
{
    /// <summary>
    /// Contains all keys that are used within the whole project for translations.
    /// </summary>
    public class Localization
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Localization"/> class.
        /// </summary>
        public Localization()
        {
            this.DutyMenu = new DutyMenuLocalization();
            this.TrafficStop = new TrafficStopLocalization();
        }

        /// <summary>Gets or sets title of the localization.</summary>
        public string Title { get; set; } = "English";

        /// <summary>Gets or sets localization of the duty menu.</summary>
        public DutyMenuLocalization DutyMenu { get; set; }

        /// <summary>Gets or sets localization of traffic stop.</summary>
        public TrafficStopLocalization TrafficStop { get; set; }
    }
}