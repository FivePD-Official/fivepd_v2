// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;

namespace FivePD.Common.Attributes
{
    /// <summary>
    /// Restricts the target to only be accessible internally.
    /// The target will be obfuscated in public builds.
    /// IMPORTANT: Make sure all applicable INTERFACES and
    /// SERVICES are marked with this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class NoUsageAttribute : Attribute
    {
    }
}