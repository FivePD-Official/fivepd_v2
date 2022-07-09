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
    public class NuiLocalization
    {
        public NuiLabels Labels { get; set; } = new NuiLabels();

        public NuiActions Actions { get; set; } = new NuiActions();
    }
}