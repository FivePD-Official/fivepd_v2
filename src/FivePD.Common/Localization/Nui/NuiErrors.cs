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
    public class NuiErrors
    {
        public string CreateDepartmentExistsWithNameTitle { get; set; } = "Couldn't create department";

        public string CreateDivisionExistsWithNameTitle { get; set; } = "Couldn't create division";

        public string CreateRankExistsWithNameTitle { get; set; } = "Couldn't create rank";

        public string DatabaseAccess { get; set; } = "A database error occured. Please try again later.";

        public string DepartmentCreate { get; set; } = "Couldn't create the department.";

        public string DepartmentExistsWithNameDescription { get; set; } = "A department with this name already exists.";

        public string DepartmentFullDescription { get; set; } = "The selected department is full.";

        public string DepartmentFullTitle { get; set; } = "Couldn't join department";

        public string DepartmentJoin { get; set; } = "Couldn't join {{department}}.";

        public string DivisionExistsWithNameDescription { get; set; } = "A division with this name already exists.";

        public string EditDepartmentExistsWithNameTitle { get; set; } = "Couldn't edit department";

        public string EditDivisionExistsWithNameTitle { get; set; } = "Couldn't rename division";

        public string EditRankExistsWithNameTitle { get; set; } = "Couldn't edit rank";

        public string FieldRequired { get; set; } = "Required.";

        public string FieldNameTaken { get; set; } = "Name taken.";

        public string FieldFormat { get; set; } = "The format is incorrect.";

        public string FieldMin { get; set; } = "The minimum value is {{value}}.";

        public string FieldMax { get; set; } = "The maximum value is {{value}}.";

        public string PleaseTryAgainLater { get; set; } = "Please try again later.";

        public string RankExistsWithNameDescription { get; set; } = "A rank with this name already exists.";

        public string UnexpectedError { get; set; } = "Unexpected error";
    }
}