using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Models.AssistanceServiceModels;

/// <summary>
/// Represents an assistance service.
/// </summary>
public abstract class AssistanceService : IInternalAssistanceService
{
    /// <summary>
    /// Gets the player who requested the assistance service.
    /// </summary>
    public FPlayer RequestedBy => ((IInternalAssistanceService)this).RequestedBy;

    /// <summary>
    /// Gets the location where the assistance service should spawn.
    /// </summary>
    public Vector3 SpawnLocation => ((IInternalAssistanceService)this).SpawnLocation;

    /// <summary>
    /// Gets the heading for the assistance service to face.
    /// </summary>
    public float SpawnHeading => ((IInternalAssistanceService)this).SpawnHeading;

    /// <summary>
    /// Gets or sets the player who requested the assistance service.
    /// </summary>
    FPlayer IInternalAssistanceService.RequestedBy { get; set; }

    /// <summary>
    /// Gets or sets the location where the assistance service should spawn.
    /// </summary>
    Vector3 IInternalAssistanceService.SpawnLocation { get; set; }

    /// <summary>
    /// Gets or sets the location where the assistance service should spawn.
    /// </summary>
    float IInternalAssistanceService.SpawnHeading { get; set; }

    /// <summary>
    /// Sets up the assistance service.
    /// </summary>
    /// <param name="cancellationTokenSource">The cancellation token source.</param>
    /// <returns>A Task.</returns>
    public abstract Task SetupAsync(CancellationTokenSource cancellationTokenSource);

    /// <summary>
    /// Determines whether the assistance service can run.
    /// </summary>
    /// <returns>A Task.</returns>
    public abstract Task<bool> CanRunAsync();

    /// <summary>
    /// Invoked when the assistance service is called.
    /// </summary>
    /// <param name="cancellationTokenSource">The cancellation token source.</param>
    /// <returns>A Task.</returns>
    public abstract Task OnCalledAsync(CancellationTokenSource cancellationTokenSource);

    /// <summary>
    /// Invoked as the last method after everything else,
    /// or when the assistance service is cancelled by a player
    /// or via the CancellationTokenSource.
    /// Does not get invoked when <see cref="AssistanceService.CanRunAsync()"/> returns false.
    /// </summary>
    /// <returns>A Task.</returns>
    public abstract Task OnCancellationRequestedAsync();

    /// <summary>
    /// Called when the assistance service is completed.
    /// </summary>
    /// <returns>A Task.</returns>
    public abstract Task OnCompletedAsync();

    /// <summary>
    /// Drives the vehicle to the target position, slowing down within a given radius.
    /// </summary>
    /// <param name="driver">The driver ped.</param>
    /// <param name="vehicle">The vehicle.</param>
    /// <param name="target">The target position.</param>
    /// <param name="minSpeed">The minimum speed.</param>
    /// <param name="maxSpeed">The maximum speed.</param>
    /// <param name="cancellationTokenSource">The cancellation token source.</param>
    /// <param name="stoppingDistance">The stopping distance squared.</param>
    /// <returns>A task.</returns>
    protected async Task DriveToTargetWithSlowingAsync(
        FPed driver,
        FVehicle vehicle,
        Vector3 target,
        float minSpeed,
        float maxSpeed,
        CancellationTokenSource cancellationTokenSource,
        float stoppingDistance = SquaredDistances._35)
    {
        const float slowingDistance = 200f;
        const float slowingDistanceSquared = SquaredDistances._200;

        driver.Task.DriveTo(vehicle.GetUnderlyingVehicle, target, 0f, maxSpeed, (int)FDrivingStyle.Emergency);

        while (vehicle.GetUnderlyingVehicle.Position.DistanceToSquared(target) >= stoppingDistance)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            var distanceSquared = vehicle.GetUnderlyingVehicle.Position.DistanceToSquared(target);
            if (distanceSquared < slowingDistanceSquared)
            {
                var distance = (float)Math.Sqrt(distanceSquared);
                var distanceRatio = Math.Min(Math.Max(distance / slowingDistance, 0f), 1f);
                var currentSpeed = minSpeed + ((maxSpeed - minSpeed) * distanceRatio);

                driver.Properties.SetDrivingSpeed(currentSpeed);
            }

            await Task.Delay(500);
        }
    }

    /// <summary>
    /// Called to relinquish the script control of entities.
    /// </summary>
    /// <param name="entities">The dictionary of entities to be relinquished, put the <see cref="FEntity"/> as the key, put the <see cref="FVehicle"/>
    /// as the value if that is the driver otherwise put null.</param>
    protected void ClearEntities(Dictionary<FEntity, FEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (entity.Key is null)
            {
                continue;
            }

            if (entity.Key.GetType() == typeof(FPed))
            {
                var castEntity = (FPed)entity.Key;
                Cfx.API.SetEntityDistanceCullingRadius(castEntity.GetUnderlyingPed.Handle, 100f);
                castEntity.Properties.MarkAsNoLongerNeeded();
                if (entity.Value is not null)
                {
                    var castVehicle = (FVehicle)entity.Value;
                    castEntity?.Task.CruiseWithVehicle(castVehicle.GetUnderlyingVehicle.NetworkId, 12f, (int)FDrivingStyle.Normal);
                }
            }
            else if (entity.Key.GetType() == typeof(FVehicle))
            {
                var castEntity = (FVehicle)entity.Key;
                castEntity.Properties.RemoveBlips();
                castEntity.Properties.IsSirenActive(false);
                Cfx.API.SetEntityDistanceCullingRadius(castEntity.GetUnderlyingVehicle.Handle, 100f);
            }
        }
    }
}

/// <summary>
/// Represents an internal assistance service.
/// </summary>
internal interface IInternalAssistanceService
{
    /// <summary>
    /// Gets or sets the player who requested the assistance service.
    /// </summary>
    public FPlayer RequestedBy { get; set; }

    /// <summary>
    /// Gets or sets the location where the assistance service should spawn.
    /// </summary>
    public Vector3 SpawnLocation { get; set; }

    /// <summary>
    /// Gets or sets the heading for the assistance service to face.
    /// </summary>
    public float SpawnHeading { get; set; }
}