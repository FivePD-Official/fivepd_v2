using FivePD.Server.API;

namespace FivePD.Server.Interfaces.Dispatcher;

#pragma warning disable

/// <summary>
/// Public interface for managing dispatcher operations.
/// </summary>
public interface IDispatcherManagerService
{
}

/// <summary>
/// Internal interface for managing dispatcher operations and incident communications.
/// </summary>
internal interface IInternalDispatcherManagerService
{
    /// <summary>
    /// Sends a new incident to all active dispatchers.
    /// </summary>
    /// <param name="incident">The callout incident to be distributed to dispatchers.</param>
    public void SendIncidentToDispatchers(Callout incident); // TODO: change to IIncident when traffic stop is done

    /// <summary>
    /// Removes an incident from all dispatchers' views.
    /// </summary>
    /// <param name="incident">The incident to be removed.</param>
    public void RemoveIncidentFromDispatchers(IIncident incident);
 
    /// <summary>
    /// Sends a request to all dispatchers to update their current state.
    /// </summary>
    public void SendUpdateRequestToDispatchers();
}