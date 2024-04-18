using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for handling scene management related tasks.
/// </summary>
public interface ISceneManagementService
{
    /// <summary>
    /// Gets the speed zones.
    /// </summary>
    /// <returns>A list of speed zones.</returns>
    public List<SpeedZone> SpeedZones();

    /// <summary>
    /// Creates a speed zone.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="speed">The maximum speed.</param>
    /// <returns>The id of the created speed zone.</returns>
    public Guid CreateSpeedZone(Vector3 position, int radius, int speed);

    /// <summary>
    /// Deletes the speed zone with the specified id.
    /// </summary>
    /// <param name="id">The id of the speed zone to delete.</param>
    /// <returns>A value indicating whether the deletion could be completed or not.</returns>
    public bool DeleteSpeedZone(Guid id);

    /// <summary>
    /// Releases the traffic for a given speed zone.
    /// </summary>
    /// <param name="id">The id of the speed zone.</param>
    public void ReleaseSpeedZoneTraffic(Guid id);
}