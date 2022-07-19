// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Common.Localization
{
    public class Localization
    {
        public string Title { get; set; } = "English";

        public DutyMenuLocalization DutyMenu { get; set; } = new DutyMenuLocalization();

        public TrafficStopLocalization TrafficStop { get; set; } = new TrafficStopLocalization();

        public ErrorLocalizations Error { get; set; } = new ErrorLocalizations();

        public WarningLocalizations Warning { get; set; } = new WarningLocalizations();

        public NuiLocalization Nui { get; set; } = new NuiLocalization();
    }
}