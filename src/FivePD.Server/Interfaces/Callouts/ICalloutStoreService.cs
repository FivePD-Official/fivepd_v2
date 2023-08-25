using System.Collections.Generic;
using FivePD.Server.API;

namespace FivePD.Server.Interfaces.Callouts;

#pragma warning disable

public interface ICalloutStoreService
{
    public List<Callout> ActiveCallouts { get; }
}

internal interface IInternalCalloutStoreService
{
    public List<CalloutHolder> ActiveCallouts { get; }
}