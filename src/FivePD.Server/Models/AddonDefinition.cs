using System.Collections.Generic;

namespace FivePD.Server.Models;

/// <summary>
/// The base model representing a gamemode extension.
/// </summary>
public class AddonDefinition
{
    /// <summary>
    /// Gets or sets the technical name for the addon. Should be PascalCase.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the friendly name for the addon. Should be Title Case.
    /// </summary>
    public string FriendlyName { get; set; }

    /// <summary>
    /// Gets or sets the machine-readable version for the addon.
    /// A higher value indicates a newer version.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the friendly version for the addon.
    /// </summary>
    public string FriendlyVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the addon is internal.
    /// Internal addons are prioritized.
    /// </summary>
    public bool IsInternal { get; set; }

    /// <summary>
    /// Gets or sets the assemblies that the addon requires.
    /// </summary>
    /// <returns>An enumerable of all assemblies which must be loaded.</returns>
    public IList<byte[]> Assemblies { get; set; }

    /// <summary>
    /// Gets or sets files.
    /// </summary>
    public IDictionary<string, string> Files { get; set; }
}