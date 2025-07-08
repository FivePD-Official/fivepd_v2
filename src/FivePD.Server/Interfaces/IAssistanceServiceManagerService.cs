using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Models;
using FivePD.Server.Models.AssistanceServiceModels;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for managing the assistance services.
/// </summary>
public interface IAssistanceServiceManagerService
{
    /// <summary>
    /// Gets all registered assistance services.
    /// </summary>
    /// <returns>A list of tuples with the type of the assistance service and its options.</returns>
    public IReadOnlyList<AvailableAssistanceService> AvailableServices { get; }

    /// <summary>
    /// Calls an assistance service.
    /// </summary>
    /// <param name="requestedBy">The player who requested the assistance service.</param>
    /// <param name="onCanceled">The action to invoke when the assistance service is canceled.</param>
    /// <typeparam name="T">The type of assistance service to call.</typeparam>
    /// <returns>The GUID of the assistance service.</returns>
    public Task<Guid> CallAsync<T>(FPlayer requestedBy, Action onCanceled = null)
        where T : AssistanceService;

    /// <summary>
    /// Calls an assistance service.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="requestedBy">The player who requested the assistance service.</param>
    /// <param name="onCanceled">The action to invoke when the assistance service is canceled.</param>
    /// <returns>The GUID of the assistance service.</returns>
    public Task<Guid> CallAsync(Type type, FPlayer requestedBy, Action onCanceled = null);

    /// <summary>
    /// Cancels an assistance service.
    /// The GUID will be set to <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="guid">The GUID of the assistance service.</param>
    public void Cancel(ref Guid guid);
}