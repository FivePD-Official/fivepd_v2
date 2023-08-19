using System.Collections.Generic;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface ICalloutStoreService
{
}

internal interface IInternalCalloutStoreService
{
    public List<CalloutHolder> ActiveCallouts { get; }
}