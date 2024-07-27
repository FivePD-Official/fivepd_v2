using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
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
    /// <returns>A Task.</returns>
    public abstract Task SetupAsync();

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
    /// Called to relinquish the script control of entities.
    /// </summary>
    /// <param name="entities">The dictionary of entities to be relinquished, the FEntity is for <see cref="FPed"/> or <see cref="FVehicle"/>,
    /// the bool is to show which <see cref="FPed"/> is the driver.</param>
    protected void ClearEntities(Dictionary<FEntity, bool> entities)
    {
        FPed driver = null;
        FVehicle vehicle = null;

        foreach (var entity in entities)
        {
            if (entity.Key.Type == "FPed")
            {
                var castEntity = (FPed)entity.Key;
                Cfx.API.SetEntityDistanceCullingRadius(castEntity.GetUnderlyingPed.Handle, 100f);
                castEntity.Properties.MarkAsNoLongerNeeded();
                if (entity.Value)
                {
                    driver = castEntity;
                }
            }
            else if (entity.Key.Type == "FVehicle")
            {
                var castEntity = (FVehicle)entity.Key;
                castEntity.Properties.RemoveBlips();
                castEntity.Properties.IsSirenActive(false);
                Cfx.API.SetEntityDistanceCullingRadius(castEntity.GetUnderlyingVehicle.Handle, 100f);
                vehicle = castEntity;
            }
        }

        this.DriveOff(vehicle, driver);
    }

    /// <summary>
    /// Used to task the vehicle to drive off.
    /// </summary>
    private void DriveOff(FVehicle vehicle, FPed driver)
    {
        if (vehicle is not null && driver is not null)
        {
            driver?.Task.CruiseWithVehicle(vehicle.GetUnderlyingVehicle, 12f, (int)FDrivingStyle.Normal);
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