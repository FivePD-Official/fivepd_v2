using FivePD.Server.API;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IDispatchService
{
}

internal interface IInternalDispatchService
{
    internal void SendCalloutToDispatchers(Callout callout);

    internal void RemoveCalloutFromDispatchers(Callout callout);
}