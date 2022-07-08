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
            this.Actions = new Actions();
        }

        /// <summary>
        /// Gets or sets all labels that can be found in the NUI.
        /// </summary>
        public Labels Labels { get; set; }

        /// <summary>
        /// Gets or sets all action labels that can be found in the NUI.
        /// </summary>
        public Actions Actions { get; set; }
    }

    public class Labels
    {
        public string Acronym { get; set; } = "Acronym";

        public string AcronymHelpLabel { get; set; } = "If left empty it'll be auto-generated from the name.";

        public string ActiveCalls { get; set; } = "Active calls";

        public string Address { get; set; } = "Address";

        public string AssignedCall { get; set; } = "Assigned call";

        public string Callsign { get; set; } = "Callsign";

        public string CallsignFormat { get; set; } = "Callsign format";

        public string CitizenDatabase { get; set; } = "Citizen database";

        public string Dashboard { get; set; } = "Dashboard";

        public string Department { get; set; } = "Department";

        public string Departments { get; set; } = "Departments";

        public string Division { get; set; } = "Division";

        public string Divisions { get; set; } = "Divisions";

        public string MaxNumberOfMembers { get; set; } = "Max number of members";

        public string Members { get; set; } = "Members";

        public string MyDetails { get; set; } = "My details";

        public string MyDepartment { get; set; } = "My department";

        public string Name { get; set; } = "Name";

        public string NewDepartment { get; set; } = "New department";

        public string NoAssignedCall { get; set; } = "You're not assigned to a call at the moment.";

        public string NoDataToDisplay { get; set; } = "No data to display.";

        public string NotInDepartment { get; set; } = "You're not in a department.";

        public string Permissions { get; set; } = "Permissions";

        public string Rank { get; set; } = "Rank";

        public string Ranks { get; set; } = "Ranks";

        public string Settings { get; set; } = "Settings";

        public string Statistics { get; set; } = "Statistics";

        public string Status { get; set; } = "Status";

        public string Type { get; set; } = "Type";

        public string Units { get; set; } = "Units";

        public string VehicleDatabase { get; set; } = "Vehicle database";
    }

    public class Actions
    {
        public string Kick { get; set; } = "Kick";

        public string Save { get; set; } = "Save";

        public string SetRank { get; set; } = "Set rank";

        public string SetCallsign { get; set; } = "Set callsign";
    }
}