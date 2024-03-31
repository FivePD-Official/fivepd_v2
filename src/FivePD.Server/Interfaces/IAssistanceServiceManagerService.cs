using System;
using System.Threading.Tasks;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Models.AssistanceServiceModels;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Represents an assistance service.
/// </summary>
public interface IAssistanceServiceManagerService
{
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
    /// Cancels an assistance service.
    /// The GUID will be set to <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="guid">The GUID of the assistance service.</param>
    public void Cancel(ref Guid guid);
}