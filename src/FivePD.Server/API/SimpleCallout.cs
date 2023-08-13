using System;
using CitizenFX.Core;
using FivePD.Common.Enums;

namespace FivePD.Server.API;

#pragma warning disable

public class SimpleCallout
{
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
    
    public string Id { get; internal set; }

    public string Title { get; internal set; }

    public string Description { get; internal set; }

    public FIncidentPriority Priority { get; internal set; }

    public float ActivityRadius { get; internal set; }

    public Vector3 Location { get; internal set; }

    public DateTime GeneratedAt { get; internal set; }
}