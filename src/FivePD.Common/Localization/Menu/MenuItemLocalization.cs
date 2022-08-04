// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Common.Localization
{
    public class MenuItemLocalization
    {
        public MenuItemLocalization(string title, string description)
        {
            this.Title = title;
            this.Description = description;
        }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}