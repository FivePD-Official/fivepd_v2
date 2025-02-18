using FivePD.Server.API;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IDispatcherManagerService
{
    public void SendIncidentToDispatchers(Callout incident); // TODO: change to IIncident when traffic stop is done

    public void RemoveIncidentFromDispatchers(IIncident incident);
 
    public void SendUpdateRequestToDispatchers();
}