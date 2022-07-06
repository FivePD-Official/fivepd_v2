// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

#pragma warning disable CS1591
#pragma warning disable SA1600
#pragma warning disable SA1402

namespace FivePD.Common.Localization
{
    /// <summary>
    /// Contains all keys that are used within the NUI.
    /// </summary>
    public class NuiLocalization
    {
        public NuiLocalization()
        {
            this.Labels = new Labels();
        }

        /// <summary>
        /// Gets or sets all labels that can be found in the NUI.
        /// </summary>
        public Labels Labels { get; set; }
    }

    public class Labels
    {
        public string Acronym { get; set; } = "Acronym";

        public string ActiveCalls { get; set; } = "Active calls";

        public string Address { get; set; } = "Address";

        public string AssignedCall { get; set; } = "Assigned call";

        public string CitizenDatabase { get; set; } = "Citizen database";

        public string Dashboard { get; set; } = "Dashboard";

        public string Department { get; set; } = "Department";

        public string Departments { get; set; } = "Departments";

        public string Division { get; set; } = "Division";

        public string Divisions { get; set; } = "Divisions";

        public string Members { get; set; } = "Members";

        public string MyDetails { get; set; } = "My details";

        public string Name { get; set; } = "Name";

        public string Rank { get; set; } = "Rank";

        public string Settings { get; set; } = "Settings";

        public string Statistics { get; set; } = "Statistics";

        public string Status { get; set; } = "Status";

        public string Type { get; set; } = "Type";

        public string Units { get; set; } = "Units";

        public string VehicleDatabase { get; set; } = "Vehicle database";
    }
}