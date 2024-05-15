using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Models.Events;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for handling scene management related tasks.
/// </summary>
public interface ISceneManagementService
{
    /// <summary>
    /// Fires when a speed zone has been created.
    /// </summary>
    public event EventHandler<SpeedZoneCreatedEventArgs> SpeedZoneCreated;

    /// <summary>
    /// Fires when a speed zone has been deleted.
    /// </summary>
    public event EventHandler<SpeedZoneDeletedEventArgs> SpeedZoneDeleted;

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

/// <summary>
/// Responsible for handling scene management related tasks.
/// </summary>
internal interface IInternalSceneManagementService
{
    /// <summary>
    /// Creates a speed zone.
    /// </summary>
    /// <param name="player">The player who created the speed zone.</param>
    /// <param name="position">The position.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="speed">The maximum speed.</param>
    /// <returns>The id of the created speed zone.</returns>
    Guid CreateSpeedZone(FPlayer player, Vector3 position, int radius, int speed);
}