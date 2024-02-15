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
    /// If <see cref="RandomPriority"/> is set to true, this property will be ignored.
    /// </summary>
    public FPriority Priority { get; init; }

    /// <summary>
    /// Gets a value indicating whether the priority should be randomized.
    /// When true, the priority will be randomly selected between code 1, 2 and 3.
    /// </summary>
    public bool RandomPriority { get; init; }

    /// <summary>
    /// Gets the activity radius of the callout.
    /// </summary>
    public required float ActivityRadius { get; init; }

    /// <summary>
    /// Gets the location of the callout.
    /// If <see cref="Locations"/> is set, this property will be ignored.
    /// </summary>
    public Vector3 Location { get; init; }

    /// <summary>
    /// Gets the locations of the callout.
    /// If this property is set, <see cref="Location"/> will be ignored.
    /// A random location will be picked from this list.
    /// </summary>
    public IEnumerable<Vector3> Locations { get; init; }

    /// <summary>
    /// Gets the minimum amount of officers required to generate the callout.
    /// </summary>
    public int MinOnDutyOfficers { get; init; }
}