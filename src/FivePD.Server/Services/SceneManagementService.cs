using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class SceneManagementService : ISceneManagementService
{
    private readonly Dictionary<Guid, SpeedZone> _speedZones = new();

    /// <inheritdoc />
    public List<SpeedZone> SpeedZones()
    {
        return this._speedZones.Select(x => x.Value).ToList();
    }

    /// <inheritdoc />
    public Guid CreateSpeedZone(Vector3 position, int radius, int speed)
    {
        var id = Guid.NewGuid();
        this._speedZones[id] = new SpeedZone
        {
            Position = position,
            Radius = radius,
            Speed = speed,
        };

        return id;
    }

    /// <inheritdoc />
    public void DeleteSpeedZone(Guid id)
    {
        this._speedZones.Remove(id);
    }

    /// <inheritdoc />
    public void ReleaseSpeedZoneTraffic(Guid id)
    {
        // a.
    }
}