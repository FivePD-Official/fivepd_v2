using System;

namespace FivePD.Common.Attributes;

/// <summary>
/// Indicates that the marked code element should be obfuscated after build.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class ObfuscateAttribute : Attribute
{
}