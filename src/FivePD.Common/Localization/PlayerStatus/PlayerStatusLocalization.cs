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
    public class PlayerStatusLocalization
    {
        public OnDutyLocalization OnDuty { get; set; } = new OnDutyLocalization();

        public OffDutyLocalization OffDuty { get; set; } = new OffDutyLocalization();
    }
}