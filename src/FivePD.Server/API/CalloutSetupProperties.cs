using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common.Enums;

namespace FivePD.Server.API;

/// <summary>
/// Represents the properties of a callout.
/// </summary>
public readonly record struct CalloutSetupProperties
{
    /// <summary>
    /// Gets the title of the callout.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the description of the callout.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Gets the priority of the callout.
    /// </summary>
    public FIncidentPriority Priority { get; init; }

    /// <summary>
    /// Gets a value indicating whether the priority should be randomized.
    /// </summary>
    public bool RandomPriority { get; init; }

    /// <summary>
    /// Gets the activity radius of the callout.
    /// </summary>
    public required float ActivityRadius { get; init; }

    /// <summary>
    /// Gets the location of the callout.
    /// </summary>
    public Vector3 Location { get; init; }

    /// <summary>
    /// Gets the locations of the callout.
    /// </summary>
    public IEnumerable<Vector3> Locations { get; init; }
}