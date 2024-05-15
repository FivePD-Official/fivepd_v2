using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers;

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