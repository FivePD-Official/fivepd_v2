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
    public class NuiLabels
    {
        public string Acronym { get; set; } = "Acronym";

        public string Actions { get; set; } = "Actions";

        public string ActiveCalls { get; set; } = "Active calls";

        public string Address { get; set; } = "Address";

        public string AssignedCall { get; set; } = "Assigned call";

        public string AvailableCharacters { get; set; } = "Available characters";

        public string Callsign { get; set; } = "Callsign";

        public string CallsignHelpLabel { get; set; } = "If left empty it'll be auto-generated from the name. Can only consists of alphabetical characters.";

        public string CallsignFormat { get; set; } = "Callsign format";

        public string CallsignFormatHelpLabel { get; set; } = "Maximum length is 10 characters.";

        public string Capacity { get; set; } = "Capacity";

        public string CitizenDatabase { get; set; } = "Citizen database";

        public string Dash { get; set; } = "Dash";

        public string Dashboard { get; set; } = "Dashboard";

        public string Department { get; set; } = "Department";

        public string Departments { get; set; } = "Departments";

        public string DeleteDivisionTitle { get; set; } = "Delete division";

        public string DeleteDivisionDescription { get; set; } = "Are you sure you want to delete {{division}}? You can't undo this action.";

        public string DeleteRankTitle { get; set; } = "Delete rank";

        public string DeleteRankDescription { get; set; } = "Are you sure you want to delete {{rank}}? You can't undo this action.";

        public string EditRankTitle { get; set; } = "Edit rank";

        public string Division { get; set; } = "Division";

        public string Divisions { get; set; } = "Divisions";

        public string Letter { get; set; } = "Letter";

        public string MaxNumberOfMembers { get; set; } = "Max number of members";

        public string MaxNumberOfMembersHelpLabel { get; set; } = "If set to zero there won't be any limit.";

        public string Members { get; set; } = "Members";

        public string MyDetails { get; set; } = "My details";

        public string MyDepartment { get; set; } = "My department";

        public string Name { get; set; } = "Name";

        public string NewDepartment { get; set; } = "New department";

        public string NoAssignedCall { get; set; } = "You're not assigned to a call at the moment.";

        public string NoDataToDisplay { get; set; } = "No data to display.";

        public string NotInDepartment { get; set; } = "You're not in a department.";

        public string NoLimit { get; set; } = "No limit";

        public string Number { get; set; } = "Number";

        public string Permissions { get; set; } = "Permissions";

        public string Rank { get; set; } = "Rank";

        public string Ranks { get; set; } = "Ranks";

        public string RenameDivision { get; set; } = "Rename division";

        public string Settings { get; set; } = "Settings";

        public string Statistics { get; set; } = "Statistics";

        public string Status { get; set; } = "Status";

        public string SuccessfulJoin { get; set; } = "Successful join";

        public string SuccessfulLeave { get; set; } = "Successful leave";

        public string SuccessfulCreate { get; set; } = "Successful create";

        public string SuccessfulDelete { get; set; } = "Successful delete";

        public string SuccessfulEdit { get; set; } = "Successful edit";

        public string SuccessfulRename { get; set; } = "Successful rename";

        public string SuccessfulDepartmentJoin { get; set; } = "You've successfully joined {{department}}.";

        public string SuccessfulDepartmentLeave { get; set; } = "You've successfully left {{department}}.";

        public string SuccessfulDepartmentCreate { get; set; } = "{{department}} has been successfully created.";

        public string SuccessfulDivisionDelete { get; set; } = "{{division}} has been successfully deleted.";

        public string SuccessfulDivisionRename { get; set; } = "{{oldDivisionName}} has been renamed to {{newDivisionName}}.";

        public string SuccessfulDivisionCreate { get; set; } = "{{division}} has been successfully created.";

        public string SuccessfulRankDelete { get; set; } = "{{rank}} has been successfully deleted.";

        public string SuccessfulRankEdit { get; set; } = "{{rank}} has been successfully edited.";

        public string SuccessfulRankCreate { get; set; } = "{{rank}} has been successfully created.";

        public string Type { get; set; } = "Type";

        public string Units { get; set; } = "Units";

        public string VehicleDatabase { get; set; } = "Vehicle database";
    }
}