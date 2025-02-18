using System;
using CitizenFX.Core;
using FivePD.Common.Enums;

namespace FivePD.Server.API;

/// <summary>
/// Represents a simplified version of a callout with basic properties.
/// </summary>
public class SimpleCallout
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleCallout"/> class.
    /// </summary>
    /// <param name="callout">The callout to create this simple callout from.</param>
    public SimpleCallout(Callout callout)
    {
        this.Id = callout.Id.ToString();
        this.Title = callout.Title;
        this.Description = callout.Description;
        this.Priority = callout.Priority;
        this.ActivityRadius = callout.ActivityRadius;
        this.Location = callout.Location;
        this.GeneratedAt = callout.GeneratedAt;
    }

    /// <summary>
    /// Gets the unique identifier of the callout.
    /// </summary>
    public string Id { get; internal set; }

    /// <summary>
    /// Gets the title of the callout.
    /// </summary>
    public string Title { get; internal set; }

    /// <summary>
    /// Gets the description of the callout.
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Gets the priority of the callout.
    /// </summary>
    public FPriority Priority { get; internal set; }

    /// <summary>
    /// Gets the activity radius of the callout.
    /// </summary>
    public float ActivityRadius { get; internal set; }

    /// <summary>
    /// Gets the location of the callout.
    /// </summary>
    public Vector3 Location { get; internal set; }

    /// <summary>
    /// Gets the date and time when the callout was generated.
    /// </summary>
    public DateTime GeneratedAt { get; internal set; }
}