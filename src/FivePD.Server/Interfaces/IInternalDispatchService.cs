using System.Collections.Generic;
using FivePD.Server.API;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IDispatchService
{
    public List<FPlayer> Dispatchers();
}

internal interface IInternalDispatchService
{
    internal void SendCalloutToDispatchers(Callout callout);

    internal void RemoveCalloutFromDispatchers(Callout callout);
}