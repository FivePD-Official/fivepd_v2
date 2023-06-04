using System;

namespace FivePD.Gamemode.Common.Attributes;

/// <summary>
/// Restricts the target to only be accessible internally.
/// The target will be obfuscated in public builds.
/// IMPORTANT: Make sure all applicable INTERFACES and
/// SERVICES are marked with this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
internal class NoUsageAttribute : Attribute
{
}